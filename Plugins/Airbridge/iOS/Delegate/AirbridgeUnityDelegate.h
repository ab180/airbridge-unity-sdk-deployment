//
//  AirbridgeUnityDelegate.h
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "AppDelegateListener.h"

NS_ASSUME_NONNULL_BEGIN

@interface AirbridgeUnityDelegate : NSObject <AppDelegateListener>

+ (AirbridgeUnityDelegate *)instance;

+ (void)setInitialized:(BOOL)initialized;
+ (void)setHandleAirbridgeDeeplinkOnly:(BOOL)enabled;
+ (BOOL)trackDeeplinkWithURL:(NSURL *)url;
+ (BOOL)trackDeeplinkWithUserActivity:(NSUserActivity *)userActivity;

+ (BOOL)trackDeeplinkWithConnectionOptions:(UISceneConnectionOptions *)connectionOptions API_AVAILABLE(ios(13.0));
+ (BOOL)trackDeeplinkWithOpenURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts API_AVAILABLE(ios(13.0));

@end

NS_ASSUME_NONNULL_END
