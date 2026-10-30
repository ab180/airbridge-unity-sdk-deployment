//
//  AirbridgeUnity+Event.h
//  UnityFramework
//
//  Created by mjgu on 8/12/24.
//

#import "AirbridgeUnity.h"

NS_ASSUME_NONNULL_BEGIN

@interface AirbridgeUnity (Event)

@end

void native_trackEvent(const char* category, const char* semanticJson, const char* customJson);

void native_setAdditionalEventAttributes(const char* typesJson, const char* semanticJson, const char* customJson);

void native_trackStoreInAppPurchaseEvent(const char* purchaseJson, const char* semanticJson, const char* customJson);

NS_ASSUME_NONNULL_END
