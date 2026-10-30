//
//  AirbridgeUnity+Permission.h
//  UnityFramework
//

#import "AirbridgeUnity.h"

NS_ASSUME_NONNULL_BEGIN

@interface AirbridgeUnity (Permission)

@end

bool native_requestTrackingAuthorization(int requestId, void (*onComplete)(int requestId, int status));

int native_trackingAuthorizationStatus(void);

NS_ASSUME_NONNULL_END
