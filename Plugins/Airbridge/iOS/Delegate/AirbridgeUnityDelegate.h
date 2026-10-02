//
//  AirbridgeUnityDelegate.h
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "AppDelegateListener.h"

#if defined(__IPHONE_13_0) || defined(__TVOS_13_0)
#define AU_UISCENE_SUPPORTED 1
#else
#define AU_UISCENE_SUPPORTED 0
#endif

NS_ASSUME_NONNULL_BEGIN

@interface AirbridgeUnityDelegate : NSObject <AppDelegateListener>

+ (AirbridgeUnityDelegate *)instance;

+ (void)setInitialized:(BOOL)initialized;
+ (void)setHandleAirbridgeDeeplinkOnly:(BOOL)enabled;
+ (BOOL)trackDeeplinkWithURL:(NSURL *)url;
+ (BOOL)trackDeeplinkWithUserActivity:(NSUserActivity *)userActivity;

#if AU_UISCENE_SUPPORTED
+ (BOOL)trackDeeplinkWithConnectionOptions:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0), tvos(13.0));
+ (BOOL)trackDeeplinkWithOpenURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0), tvos(13.0));
#endif

@end

NS_ASSUME_NONNULL_END
