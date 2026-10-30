//
//  AirbridgeUnity+Permission.m
//  UnityFramework
//

#import "AirbridgeUnity+Permission.h"

#import <Airbridge/Airbridge.h>

@implementation AirbridgeUnity (Permission)

@end

bool native_requestTrackingAuthorization(int requestId, void (*onComplete)(int requestId, int status)) {
    return [Airbridge requestTrackingAuthorizationWithCompletionHandler:^(AirbridgeTrackingAuthorization status) {
        onComplete(requestId, (int)status);
    }];
}

int native_trackingAuthorizationStatus(void) {
    return (int)[Airbridge trackingAuthorizationStatus];
}
