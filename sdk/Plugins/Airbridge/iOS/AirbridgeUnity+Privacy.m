//
//  AirbridgeUnity+Privacy.m
//  UnityFramework
//
//  Created by mjgu on 8/12/24.
//

#import "AirbridgeUnity+Privacy.h"

#import <Airbridge/Airbridge.h>

@implementation AirbridgeUnity (Privacy)

@end

void native_startTracking(void) {
    [Airbridge startTracking];
}

void native_stopTracking(void) {
    [Airbridge stopTracking];
}

bool native_isTrackingEnabled(void) {
    return [Airbridge isTrackingEnabled];
}

void native_blockTrackingItem(int item) {
    AirbridgeTrackingBlocklist* blocklistItem;
    switch (item) {
        case 0:
            blocklistItem = AirbridgeTrackingBlocklist.idfa;
            break;
        case 1:
            blocklistItem = AirbridgeTrackingBlocklist.idfv;
            break;
        case 2:
            blocklistItem = AirbridgeTrackingBlocklist.skAdNetwork;
            break;
        default:
            return;
    }
    [Airbridge blockTrackingItem:blocklistItem];
}

void native_allowTrackingItem(int item) {
    AirbridgeTrackingBlocklist* blocklistItem;
    switch (item) {
        case 0:
            blocklistItem = AirbridgeTrackingBlocklist.idfa;
            break;
        case 1:
            blocklistItem = AirbridgeTrackingBlocklist.idfv;
            break;
        case 2:
            blocklistItem = AirbridgeTrackingBlocklist.skAdNetwork;
            break;
        default:
            return;
    }
    [Airbridge allowTrackingItem:blocklistItem];
}
