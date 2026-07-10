//
//  AirbridgeUnityDelegate.mm
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import "AirbridgeUnityDelegate.h"

#import <Airbridge/Airbridge.h>
#import <objc/runtime.h>

#import "AirbridgeUnity.h"
#import "Libraries/Plugins/iOS/Airbridge/AUAppSetting.h"

#if defined(__IPHONE_13_0) || defined(__TVOS_13_0)
#define AU_UISCENE_SUPPORTED 1
#else
#define AU_UISCENE_SUPPORTED 0
#endif

#if AU_UISCENE_SUPPORTED
API_AVAILABLE(ios(13.0), tvos(13.0))
typedef void (*AUSceneWillConnectIMP)(id, SEL, UIScene *, UISceneSession *, UISceneConnectionOptions *);

API_AVAILABLE(ios(13.0), tvos(13.0))
typedef void (*AUSceneOpenURLContextsIMP)(id, SEL, UIScene *, NSSet<UIOpenURLContext *> *);

API_AVAILABLE(ios(13.0), tvos(13.0))
typedef void (*AUSceneContinueUserActivityIMP)(id, SEL, UIScene *, NSUserActivity *);

static NSString *const kAUSceneDelegatePrefix = @"AirbridgeSwizzling_";
static char kAUSceneDelegatePreviousImplementationsKey;
static char kAUSceneDelegateRealClassKey;
#endif

static NSMutableSet<NSString *> *activeSceneDeeplinkKeys;

@implementation AirbridgeUnityDelegate

static AirbridgeUnityDelegate* shared = AirbridgeUnityDelegate.instance;
static BOOL isInitialized = NO;

+ (void)initialize {
    if (shared == nil) {
        shared = AirbridgeUnityDelegate.instance;
    }
}

+ (AirbridgeUnityDelegate *)instance {
    if (shared == nil) {
        shared = [[AirbridgeUnityDelegate alloc] init];
    }

    return shared;
}

+ (void)setInitialized:(BOOL)initialized {
    isInitialized = initialized;
}

- (instancetype) init {
    self = [super init];
    if (!self) {
        return nil;
    }

    UnityRegisterAppDelegateListener(self);

#if AU_UISCENE_SUPPORTED
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if (@available(iOS 13.0, tvOS 13.0, *)) {
            [[NSNotificationCenter defaultCenter]
                addObserver:self
                   selector:@selector(handleSceneWillConnectToNotification:)
                       name:UISceneWillConnectNotification
                     object:nil];
        }
    });
#endif

    return self;
}

+ (void)sendDeeplinkToUnity:(NSString *)deeplink {
    if (AirbridgeUnity.sharedInstance.deeplinkOnReceived == nil) {
        AirbridgeUnity.sharedInstance.initializeBeforeDeeplinkString = deeplink;
    } else {
        AirbridgeUnity.sharedInstance.deeplinkOnReceived(deeplink);
    }
}

+ (NSMutableSet<NSString *> *)activeSceneDeeplinkIdentifiers {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        activeSceneDeeplinkKeys = [[NSMutableSet alloc] init];
    });
    return activeSceneDeeplinkKeys;
}

+ (NSString *)deeplinkIdentifierWithURL:(NSURL *)url {
    if (url == nil) { return nil; }
    return [NSString stringWithFormat:@"url:%@", url.absoluteString];
}

+ (NSString *)deeplinkIdentifierWithUserActivity:(NSUserActivity *)userActivity {
    if (userActivity.webpageURL == nil) { return nil; }
    return [NSString stringWithFormat:@"userActivity:%@", userActivity.webpageURL.absoluteString];
}

+ (BOOL)isActiveSceneDeeplinkIdentifier:(NSString *)identifier {
    if (identifier == nil) { return NO; }

    NSMutableSet<NSString *> *identifiers = [AirbridgeUnityDelegate activeSceneDeeplinkIdentifiers];
    @synchronized (identifiers) {
        return [identifiers containsObject:identifier];
    }
}

+ (void)performWithActiveSceneDeeplinkIdentifiers:(NSSet<NSString *> *)deeplinkIdentifiers
                                           block:(void (^)(void))block {
    if (deeplinkIdentifiers.count == 0) {
        block();
        return;
    }

    NSMutableSet<NSString *> *activeIdentifiers = [AirbridgeUnityDelegate activeSceneDeeplinkIdentifiers];
    @synchronized (activeIdentifiers) {
        [activeIdentifiers unionSet:deeplinkIdentifiers];
    }
    block();
    @synchronized (activeIdentifiers) {
        [activeIdentifiers minusSet:deeplinkIdentifiers];
    }
}

+ (BOOL)trackDeeplinkWithURL:(NSURL *)url {
    if (!isInitialized) { return NO; }

    NSString *identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithURL:url];
    if ([AirbridgeUnityDelegate isActiveSceneDeeplinkIdentifier:identifier]) {
        return YES;
    }

    [Airbridge trackDeeplinkWithUrl:url];
    BOOL isHandled = [Airbridge handleDeeplinkWithUrl:url onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || isHandleAirbridgeDeeplinkOnly) {
        return YES;
    }

    [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    return YES;
}

+ (BOOL)trackDeeplinkWithUserActivity:(NSUserActivity *)userActivity {
    if (!isInitialized) { return NO; }

    NSString *identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithUserActivity:userActivity];
    if ([AirbridgeUnityDelegate isActiveSceneDeeplinkIdentifier:identifier]) {
        return YES;
    }

    [Airbridge trackDeeplinkWithUserActivity:userActivity];
    BOOL isHandled = [Airbridge handleDeeplinkWithUserActivity:userActivity onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || isHandleAirbridgeDeeplinkOnly) {
        return YES;
    }

    [AirbridgeUnityDelegate sendDeeplinkToUnity:userActivity.webpageURL.absoluteString];
    return YES;
}

#if AU_UISCENE_SUPPORTED
+ (BOOL)trackDeeplinkWithConnectionOptions:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0), tvos(13.0)) {
    if (!isInitialized) { return NO; }
    [Airbridge trackDeeplinkWithConnectionOptions:connectionOptions];
    BOOL isHandled = [Airbridge handleDeeplinkWithConnectionOptions:connectionOptions onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || isHandleAirbridgeDeeplinkOnly) {
        return YES;
    }

    NSURL *url = connectionOptions.URLContexts.anyObject.URL;
    if (url == nil) {
        NSUserActivity *userActivity = connectionOptions.userActivities.anyObject;
        url = userActivity.webpageURL;
    }
    if (url != nil) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }
    return YES;
}

+ (BOOL)trackDeeplinkWithOpenURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0), tvos(13.0)) {
    if (!isInitialized) { return NO; }
    [Airbridge trackDeeplinkWithOpenURLContexts:URLContexts];
    BOOL isHandled = [Airbridge handleDeeplinkWithOpenURLContexts:URLContexts onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || isHandleAirbridgeDeeplinkOnly) {
        return YES;
    }

    for (UIOpenURLContext *URLContext in URLContexts) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:URLContext.URL.absoluteString];
    }
    return YES;
}
#endif

- (void)didFinishLaunching:(NSNotification *)notification {
    if (appToken == nil || [appToken isEqualToString:@""]) { return; }
    if (appName == nil || [appName isEqualToString:@""]) { return; }

    [AirbridgeUnity.sharedInstance initializeSDK];
    [AirbridgeUnityDelegate setInitialized:YES];
}

#if AU_UISCENE_SUPPORTED
- (void)handleSceneWillConnectToNotification:(NSNotification *)notification {
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        if ([notification.object isKindOfClass:[UIScene class]]) {
            UIScene *scene = (UIScene *)notification.object;
            [AirbridgeUnityDelegate swizzleSceneDelegateIfNeeded:scene];
        }
    }
}

+ (IMP)previousImplementationForSelector:(SEL)selector object:(id)object {
    NSDictionary<NSString *, NSValue *> *previousImplementationsBySelector =
        objc_getAssociatedObject(object, &kAUSceneDelegatePreviousImplementationsKey);
    NSValue *previousImplementationValue =
        previousImplementationsBySelector[NSStringFromSelector(selector)];
    if (previousImplementationValue == nil) {
        return NULL;
    }

    IMP previousImplementation = NULL;
    [previousImplementationValue getValue:&previousImplementation];
    return previousImplementation;
}

+ (void)addSceneDelegateMethodWithDestinationSelector:(SEL)destinationSelector
                                       sourceSelector:(SEL)sourceSelector
                                            fromClass:(Class)sourceClass
                                              toClass:(Class)destinationClass
                                            realClass:(Class)realClass
                    previousImplementationsBySelector:(NSMutableDictionary<NSString *, NSValue *> *)previousImplementationsBySelector {
    Method sourceMethod = class_getInstanceMethod(sourceClass, sourceSelector);
    if (sourceMethod == NULL) {
        return;
    }

    IMP sourceImplementation = method_getImplementation(sourceMethod);
    const char *types = method_getTypeEncoding(sourceMethod);
    class_addMethod(destinationClass, destinationSelector, sourceImplementation, types);

    Method previousMethod = class_getInstanceMethod(realClass, destinationSelector);
    if (previousMethod == NULL) {
        return;
    }

    IMP previousImplementation = method_getImplementation(previousMethod);
    previousImplementationsBySelector[NSStringFromSelector(destinationSelector)] =
        [NSValue valueWithBytes:&previousImplementation objCType:@encode(IMP)];
}

+ (void)swizzleSceneDelegateIfNeeded:(UIScene *)scene API_AVAILABLE(ios(13.0), tvos(13.0)) {
    id<UISceneDelegate> sceneDelegate = scene.delegate;
    if (sceneDelegate == nil) {
        return;
    }

    Class realClass = [sceneDelegate class];
    NSString *className = NSStringFromClass(realClass);
    if (className == nil || [className hasPrefix:kAUSceneDelegatePrefix]) {
        return;
    }

    NSString *newClassName =
        [NSString stringWithFormat:@"%@%@-%@", kAUSceneDelegatePrefix, className, [NSUUID UUID].UUIDString];
    if (NSClassFromString(newClassName)) {
        return;
    }

    Class sceneDelegateSubClass = objc_allocateClassPair(realClass, newClassName.UTF8String, 0);
    if (sceneDelegateSubClass == Nil) {
        return;
    }

    NSMutableDictionary<NSString *, NSValue *> *previousImplementationsBySelector =
        [[NSMutableDictionary alloc] init];

    [self addSceneDelegateMethodWithDestinationSelector:@selector(scene:willConnectToSession:options:)
                                         sourceSelector:@selector(airbridgeSwizzling_scene:willConnectToSession:options:)
                                              fromClass:[AirbridgeUnityDelegate class]
                                                toClass:sceneDelegateSubClass
                                              realClass:realClass
                      previousImplementationsBySelector:previousImplementationsBySelector];
    [self addSceneDelegateMethodWithDestinationSelector:@selector(scene:openURLContexts:)
                                         sourceSelector:@selector(airbridgeSwizzling_scene:openURLContexts:)
                                              fromClass:[AirbridgeUnityDelegate class]
                                                toClass:sceneDelegateSubClass
                                              realClass:realClass
                      previousImplementationsBySelector:previousImplementationsBySelector];
    [self addSceneDelegateMethodWithDestinationSelector:@selector(scene:continueUserActivity:)
                                         sourceSelector:@selector(airbridgeSwizzling_scene:continueUserActivity:)
                                              fromClass:[AirbridgeUnityDelegate class]
                                                toClass:sceneDelegateSubClass
                                              realClass:realClass
                      previousImplementationsBySelector:previousImplementationsBySelector];

    if (class_getInstanceSize(realClass) != class_getInstanceSize(sceneDelegateSubClass)) {
        objc_disposeClassPair(sceneDelegateSubClass);
        return;
    }

    objc_setAssociatedObject(sceneDelegate, &kAUSceneDelegatePreviousImplementationsKey,
                             [previousImplementationsBySelector copy], OBJC_ASSOCIATION_RETAIN_NONATOMIC);
    objc_setAssociatedObject(sceneDelegate, &kAUSceneDelegateRealClassKey, realClass,
                             OBJC_ASSOCIATION_ASSIGN);

    objc_registerClassPair(sceneDelegateSubClass);
    object_setClass(sceneDelegate, sceneDelegateSubClass);
}

+ (NSSet<NSString *> *)deeplinkIdentifiersWithConnectionOptions:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0), tvos(13.0)) {
    NSString *identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithURL:connectionOptions.URLContexts.anyObject.URL];
    if (identifier != nil) {
        return [NSSet setWithObject:identifier];
    }

    NSUserActivity *userActivity = connectionOptions.userActivities.anyObject;
    identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithUserActivity:userActivity];
    if (identifier == nil) {
        return [NSSet set];
    }
    return [NSSet setWithObject:identifier];
}

+ (NSSet<NSString *> *)deeplinkIdentifiersWithURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0), tvos(13.0)) {
    NSMutableSet<NSString *> *identifiers = [[NSMutableSet alloc] init];
    for (UIOpenURLContext *URLContext in URLContexts) {
        NSString *identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithURL:URLContext.URL];
        if (identifier != nil) {
            [identifiers addObject:identifier];
        }
    }
    return [identifiers copy];
}

+ (NSSet<NSString *> *)deeplinkIdentifiersWithUserActivity:(NSUserActivity *)userActivity {
    NSString *identifier = [AirbridgeUnityDelegate deeplinkIdentifierWithUserActivity:userActivity];
    if (identifier == nil) {
        return [NSSet set];
    }
    return [NSSet setWithObject:identifier];
}

+ (void)callPreviousSceneWillConnectForObject:(id)object
                                        scene:(UIScene *)scene
                                      session:(UISceneSession *)session
                                      options:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0), tvos(13.0)) {
    SEL selector = @selector(scene:willConnectToSession:options:);
    AUSceneWillConnectIMP previousImplementation =
        (AUSceneWillConnectIMP)[AirbridgeUnityDelegate previousImplementationForSelector:selector object:object];

    if (previousImplementation != NULL) {
        previousImplementation(object, selector, scene, session, connectionOptions);
    }
}

+ (void)callPreviousSceneOpenURLContextsForObject:(id)object
                                            scene:(UIScene *)scene
                                      URLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0), tvos(13.0)) {
    SEL selector = @selector(scene:openURLContexts:);
    AUSceneOpenURLContextsIMP previousImplementation =
        (AUSceneOpenURLContextsIMP)[AirbridgeUnityDelegate previousImplementationForSelector:selector object:object];

    if (previousImplementation != NULL) {
        previousImplementation(object, selector, scene, URLContexts);
    }
}

+ (void)callPreviousSceneContinueUserActivityForObject:(id)object
                                                 scene:(UIScene *)scene
                                          userActivity:(NSUserActivity *)userActivity API_AVAILABLE(ios(13.0), tvos(13.0)) {
    SEL selector = @selector(scene:continueUserActivity:);
    AUSceneContinueUserActivityIMP previousImplementation =
        (AUSceneContinueUserActivityIMP)[AirbridgeUnityDelegate previousImplementationForSelector:selector object:object];

    if (previousImplementation != NULL) {
        previousImplementation(object, selector, scene, userActivity);
    }
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
            willConnectToSession:(UISceneSession *)session
                         options:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0), tvos(13.0)) {
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        NSSet<NSString *> *deeplinkIdentifiers =
            [AirbridgeUnityDelegate deeplinkIdentifiersWithConnectionOptions:connectionOptions];
        [AirbridgeUnityDelegate performWithActiveSceneDeeplinkIdentifiers:deeplinkIdentifiers block:^{
            [AirbridgeUnityDelegate
             callPreviousSceneWillConnectForObject:self
             scene:scene
             session:session
             options:connectionOptions];
        }];
        [AirbridgeUnityDelegate trackDeeplinkWithConnectionOptions:connectionOptions];
    }
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
                 openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0), tvos(13.0)) {
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        NSSet<NSString *> *deeplinkIdentifiers =
            [AirbridgeUnityDelegate deeplinkIdentifiersWithURLContexts:URLContexts];
        [AirbridgeUnityDelegate performWithActiveSceneDeeplinkIdentifiers:deeplinkIdentifiers block:^{
            [AirbridgeUnityDelegate
             callPreviousSceneOpenURLContextsForObject:self
             scene:scene
             URLContexts:URLContexts];
        }];
        [AirbridgeUnityDelegate trackDeeplinkWithOpenURLContexts:URLContexts];
    }
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
            continueUserActivity:(NSUserActivity *)userActivity API_AVAILABLE(ios(13.0), tvos(13.0)) {
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        NSSet<NSString *> *deeplinkIdentifiers =
            [AirbridgeUnityDelegate deeplinkIdentifiersWithUserActivity:userActivity];
        [AirbridgeUnityDelegate performWithActiveSceneDeeplinkIdentifiers:deeplinkIdentifiers block:^{
            [AirbridgeUnityDelegate
             callPreviousSceneContinueUserActivityForObject:self
             scene:scene
             userActivity:userActivity];
        }];
        [AirbridgeUnityDelegate trackDeeplinkWithUserActivity:userActivity];
    }
}
#endif

@end
