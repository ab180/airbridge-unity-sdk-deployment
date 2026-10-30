//
//  AirbridgeUnityDelegate.mm
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import "AirbridgeUnityDelegate.h"

#import <Airbridge/Airbridge.h>

#import "AirbridgeUnity.h"
#import "../Utils/AUSwizzling.h"
#import "Libraries/Plugins/iOS/Airbridge/AUAppSetting.h"

#define AU_SCENE_WILL_CONNECT_SELECTOR                     @selector(scene:willConnectToSession:options:)
#define AU_SCENE_OPEN_URL_CONTEXTS_SELECTOR                @selector(scene:openURLContexts:)
#define AU_SCENE_CONTINUE_USER_ACTIVITY_SELECTOR           @selector(scene:continueUserActivity:)

#define AU_SWIZZLING_SCENE_WILL_CONNECT_SELECTOR           @selector(airbridgeSwizzling_scene:willConnectToSession:options:)
#define AU_SWIZZLING_SCENE_OPEN_URL_CONTEXTS_SELECTOR      @selector(airbridgeSwizzling_scene:openURLContexts:)
#define AU_SWIZZLING_SCENE_CONTINUE_USER_ACTIVITY_SELECTOR @selector(airbridgeSwizzling_scene:continueUserActivity:)

API_AVAILABLE(ios(13.0))
typedef void (*AUSceneWillConnectIMP)(id, SEL, UIScene *, UISceneSession *, UISceneConnectionOptions *);

API_AVAILABLE(ios(13.0))
typedef void (*AUSceneOpenURLContextsIMP)(id, SEL, UIScene *, NSSet<UIOpenURLContext *> *);

API_AVAILABLE(ios(13.0))
typedef void (*AUSceneContinueUserActivityIMP)(id, SEL, UIScene *, NSUserActivity *);

@implementation AirbridgeUnityDelegate

static AirbridgeUnityDelegate* shared = AirbridgeUnityDelegate.instance;
static BOOL isInitialized = NO;
static NSNumber *configuredHandleAirbridgeDeeplinkOnly;

+ (void)initialize {
    if (shared) { return; }

    shared = AirbridgeUnityDelegate.instance;
}

+ (AirbridgeUnityDelegate *)instance {
    if (shared) { return shared; }

    shared = [[AirbridgeUnityDelegate alloc] init];

    return shared;
}

- (instancetype) init {
    self = [super init];
    if (!self) { return nil; }

    UnityRegisterAppDelegateListener(self);

    return self;
}

+ (void)setHandleAirbridgeDeeplinkOnly:(BOOL)enabled {
    configuredHandleAirbridgeDeeplinkOnly = @(enabled);
}

+ (void)setInitialized:(BOOL)initialized {
    isInitialized = initialized;
}

+ (BOOL)handleAirbridgeDeeplinkOnly {
    return configuredHandleAirbridgeDeeplinkOnly
        ? configuredHandleAirbridgeDeeplinkOnly.boolValue : isHandleAirbridgeDeeplinkOnly;
}

+ (void)sendDeeplinkToUnity:(NSString *)deeplink {
    if (!AirbridgeUnity.sharedInstance.deeplinkOnReceived) {
        AirbridgeUnity.sharedInstance.initializeBeforeDeeplinkString = deeplink;
        return;
    }

    AirbridgeUnity.sharedInstance.deeplinkOnReceived(deeplink);
}

+ (BOOL)trackDeeplinkWithURL:(NSURL *)url {
    if (!isInitialized) { return NO; }

    [Airbridge trackDeeplinkWithUrl:url];
    BOOL isHandled = [Airbridge handleDeeplinkWithUrl:url onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || [self handleAirbridgeDeeplinkOnly]) { return YES; }

    [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    return YES;
}

+ (BOOL)trackDeeplinkWithUserActivity:(NSUserActivity *)userActivity {
    if (!isInitialized) { return NO; }

    [Airbridge trackDeeplinkWithUserActivity:userActivity];
    BOOL isHandled = [Airbridge handleDeeplinkWithUserActivity:userActivity onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || [self handleAirbridgeDeeplinkOnly]) { return YES; }

    [AirbridgeUnityDelegate sendDeeplinkToUnity:userActivity.webpageURL.absoluteString];
    return YES;
}

+ (BOOL)trackDeeplinkWithConnectionOptions:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0)) {
    if (!isInitialized) { return NO; }

    [Airbridge trackDeeplinkWithConnectionOptions:connectionOptions];
    BOOL isHandled = [Airbridge handleDeeplinkWithConnectionOptions:connectionOptions onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || [self handleAirbridgeDeeplinkOnly]) { return YES; }

    NSURL *url = connectionOptions.URLContexts.anyObject.URL;
    if (!url) {
        NSUserActivity *userActivity = connectionOptions.userActivities.anyObject;
        url = userActivity.webpageURL;
    }

    if (!url) { return YES; }

    [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    return YES;
}

+ (BOOL)trackDeeplinkWithOpenURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0)) {
    if (!isInitialized) { return NO; }

    [Airbridge trackDeeplinkWithOpenURLContexts:URLContexts];
    BOOL isHandled = [Airbridge handleDeeplinkWithOpenURLContexts:URLContexts onSuccess:^(NSURL * _Nonnull url) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:url.absoluteString];
    }];

    if (isHandled || [self handleAirbridgeDeeplinkOnly]) { return YES; }

    for (UIOpenURLContext *URLContext in URLContexts) {
        [AirbridgeUnityDelegate sendDeeplinkToUnity:URLContext.URL.absoluteString];
    }
    return YES;
}

- (void)didFinishLaunching:(NSNotification *)notification {
    if (@available(iOS 13.0, *)) {
        [AirbridgeUnityDelegate installSceneDelegateHooks];
    }

    if (!appToken || [appToken isEqualToString:@""]) { return; }
    if (!appName || [appName isEqualToString:@""]) { return; }

    [AirbridgeUnity.sharedInstance initializeSDK];
    [AirbridgeUnityDelegate setInitialized:YES];
}

+ (void)installSceneDelegateHooks API_AVAILABLE(ios(13.0)) {
    NSDictionary *manifest = NSBundle.mainBundle.infoDictionary[@"UIApplicationSceneManifest"];
    if (!manifest) { return; }

    NSDictionary *configurations = manifest[@"UISceneConfigurations"];
    if (!configurations) { return; }

    for (NSArray *roleConfigurations in configurations.allValues) {
        for (NSDictionary *configuration in roleConfigurations) {
            NSString *className = configuration[@"UISceneDelegateClassName"];
            if (className.length == 0) { continue; }

            Class delegateClass = NSClassFromString(className);
            if (!delegateClass) { continue; }

            [self swizzleSceneDelegateClassIfNeeded:delegateClass];
        }
    }
}

+ (void)swizzleSceneDelegateClassIfNeeded:(Class)delegateClass API_AVAILABLE(ios(13.0)) {
    NSString *originalWillConnectSelector = NSStringFromSelector(AU_SCENE_WILL_CONNECT_SELECTOR);
    NSString *replacementWillConnectSelector = NSStringFromSelector(AU_SWIZZLING_SCENE_WILL_CONNECT_SELECTOR);

    NSString *originalOpenURLContextsSelector = NSStringFromSelector(AU_SCENE_OPEN_URL_CONTEXTS_SELECTOR);
    NSString *replacementOpenURLContextsSelector = NSStringFromSelector(AU_SWIZZLING_SCENE_OPEN_URL_CONTEXTS_SELECTOR);

    NSString *originalContinueUserActivitySelector = NSStringFromSelector(AU_SCENE_CONTINUE_USER_ACTIVITY_SELECTOR);
    NSString *replacementContinueUserActivitySelector = NSStringFromSelector(AU_SWIZZLING_SCENE_CONTINUE_USER_ACTIVITY_SELECTOR);

    NSDictionary<NSString *, NSString *> *selectors = @{
        originalWillConnectSelector: replacementWillConnectSelector,
        originalOpenURLContextsSelector: replacementOpenURLContextsSelector,
        originalContinueUserActivitySelector: replacementContinueUserActivitySelector
    };

    [AUSwizzling swizzleClassIfNeeded:delegateClass
                         sourceClass:[AirbridgeUnityDelegate class]
                           selectors:selectors];
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
            willConnectToSession:(UISceneSession *)session
                         options:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0)) {
    [AirbridgeUnityDelegate trackDeeplinkWithConnectionOptions:connectionOptions];

    [AUSwizzling
        invokePreviousImplementationForSelector:AU_SCENE_WILL_CONNECT_SELECTOR
        object:self
        invocation:^(IMP implementation, SEL selector) {
            ((AUSceneWillConnectIMP)implementation)(
                self, selector, scene, session, connectionOptions
            );
        }
    ];
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
                 openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0)) {
    [AirbridgeUnityDelegate trackDeeplinkWithOpenURLContexts:URLContexts];

    [AUSwizzling
        invokePreviousImplementationForSelector:AU_SCENE_OPEN_URL_CONTEXTS_SELECTOR
        object:self
        invocation:^(IMP implementation, SEL selector) {
            ((AUSceneOpenURLContextsIMP)implementation)(
                self, selector, scene, URLContexts
            );
        }
    ];
}

- (void)airbridgeSwizzling_scene:(UIScene *)scene
            continueUserActivity:(NSUserActivity *)userActivity API_AVAILABLE(ios(13.0)) {
    [AirbridgeUnityDelegate trackDeeplinkWithUserActivity:userActivity];

    [AUSwizzling
        invokePreviousImplementationForSelector:AU_SCENE_CONTINUE_USER_ACTIVITY_SELECTOR
        object:self
        invocation:^(IMP implementation, SEL selector) {
            ((AUSceneContinueUserActivityIMP)implementation)(
                self, selector, scene, userActivity
            );
        }
    ];
}

@end
