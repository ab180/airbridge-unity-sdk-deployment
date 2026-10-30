//
//  AirbridgeUnity+Event.m
//  UnityFramework
//
//  Created by mjgu on 8/12/24.
//

#import "AirbridgeUnity+Event.h"

#import <Airbridge/Airbridge.h>

#import "AUConvert.h"

@implementation AirbridgeUnity (Event)

@end

void native_trackEvent(const char* category, const char* semanticJson, const char* customJson) {
    NSString *categoryString = [AUConvert stringFromChars:category];
    NSDictionary *semanticAttributes = [AUConvert dictionaryFromJSONChars:semanticJson];
    NSDictionary *customAttributes = [AUConvert dictionaryFromJSONChars:customJson];
    
    [Airbridge
     trackEventWithCategory:categoryString
     semanticAttributes:semanticAttributes
     customAttributes:customAttributes
    ];
}

void native_setAdditionalEventAttributes(const char* typesJson, const char* semanticJson, const char* customJson) {
    NSArray *typeNames = [AUConvert arrayFromJSONChars:typesJson];
    NSDictionary *semanticAttributes = [AUConvert dictionaryFromJSONChars:semanticJson];
    NSDictionary *customAttributes = [AUConvert dictionaryFromJSONChars:customJson];

    // The bridge passes the enum name, and the Swift cases are camelCase, so the names are mapped here.
    NSDictionary<NSString*, NSNumber*> *eventTypes = @{
        @"INSTALL": @(AirbridgeEventTypeInstall),
        @"OPEN": @(AirbridgeEventTypeOpen),
        @"DEEPLINK_OPEN": @(AirbridgeEventTypeDeeplinkOpen)
    };

    // The SDK exposes the array overload to Swift only, so each type is applied one by one.
    for (id typeName in typeNames) {
        NSNumber *type = [typeName isKindOfClass:[NSString class]]
            ? [eventTypes valueForKey:typeName]
            : nil;
        if (type == nil) {
            NSLog(@"Unknown event type: %@", typeName);
            continue;
        }

        [Airbridge
         setAdditionalEventAttributesWithType:(AirbridgeEventType)type.integerValue
         semanticAttributes:semanticAttributes
         customAttributes:customAttributes
        ];
    }
}

void native_trackStoreInAppPurchaseEvent(const char* purchaseJson, const char* semanticJson, const char* customJson) {
    NSDictionary *purchase = [AUConvert dictionaryFromJSONChars:purchaseJson];
    if (purchase == nil) { return; }

    NSDictionary *semanticAttributes = [AUConvert dictionaryFromJSONChars:semanticJson];
    NSDictionary *customAttributes = [AUConvert dictionaryFromJSONChars:customJson];

    [Airbridge
     trackStoreInAppPurchaseEvent:[[AirbridgeStoreInAppPurchase alloc] initWithObject:purchase]
     semanticAttributes:semanticAttributes
     customAttributes:customAttributes
    ];
}
