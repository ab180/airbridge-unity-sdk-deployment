//
//  AirbridgeUnity+Privacy.h
//  UnityFramework
//
//  Created by mjgu on 8/12/24.
//

#import "AirbridgeUnity.h"

NS_ASSUME_NONNULL_BEGIN

@interface AirbridgeUnity (Privacy)

@end

void native_startTracking(void);
void native_stopTracking(void);
bool native_isTrackingEnabled(void);

void native_blockTrackingItem(int item);
void native_allowTrackingItem(int item);

NS_ASSUME_NONNULL_END
