//
//  AUAppController.mm
//  AirbridgeUnity
//
//  Created by WOF on 2019/12/10.
//  Copyright © 2019 ab180. All rights reserved.
//

#import "UnityAppController.h"

#import "AirbridgeUnityDelegate.h"

@interface AUAppController : UnityAppController

@end

@implementation AUAppController

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
    [super application:app openURL:url options:options];
    return [AirbridgeUnityDelegate trackDeeplinkWithURL:url];
}

- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring>>* _Nullable))restorationHandler {
    [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
    return [AirbridgeUnityDelegate trackDeeplinkWithUserActivity:userActivity];
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(AUAppController)
