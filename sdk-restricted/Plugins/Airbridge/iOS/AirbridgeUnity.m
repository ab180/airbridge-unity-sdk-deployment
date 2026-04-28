//
//  AirbridgeUnity.h
//  AirbridgeUnity
//
//  Created by WOF on 29/11/2019.
//

#import "AirbridgeUnity.h"

#import <Airbridge/Airbridge.h>

#import "AUConvert.h"
#import "Libraries/Plugins/iOS/Airbridge/AUAppSetting.h"

@implementation AirbridgeUnity

+ (instancetype)sharedInstance
{
    static AirbridgeUnity *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (void)initializeSDK {
    AirbridgeOptionBuilder *builder = [[AirbridgeOptionBuilder alloc]
        initWithName:appName
        token:appToken
    ];
    
    NSArray *customDomains = [customDomain componentsSeparatedByString:@" "];
    
    NSArray *trackingBlocklistStrings = [trackingBlocklist componentsSeparatedByString:@","];
    NSMutableArray *trackingBlocklists = [NSMutableArray array];
    for (NSString *trackingBlocklistString in trackingBlocklistStrings) {
        NSString *trimmedTrackingBlocklistString =
            [trackingBlocklistString stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
        
        if (trimmedTrackingBlocklistString.length == 0) continue;
        
        if ([trimmedTrackingBlocklistString caseInsensitiveCompare:@"idfa"] == NSOrderedSame) {
            [trackingBlocklists addObject:AirbridgeTrackingBlocklist.idfa];
        } else if ([trimmedTrackingBlocklistString caseInsensitiveCompare:@"idfv"] == NSOrderedSame) {
            [trackingBlocklists addObject:AirbridgeTrackingBlocklist.idfv];
        }
    }
    
    builder = [builder setLogLevel:logLevel];
    builder = [builder setSDKDevelopmentPlatform:@"unity"];
    if (customDomains.count != 0) {
        builder = [builder setTrackingLinkCustomDomains:customDomains];
    }
    builder = [builder setSessionTimeoutWithSecond:sessionTimeoutSeconds];
    builder = [builder setAutoStartTrackingEnabled:autoStartTrackingEnabled];
    builder = [builder setHashUserInformationEnabled:userInfoHashEnabled];
    builder = [builder setTrackAirbridgeDeeplinkOnlyEnabled:trackAirbridgeLinkOnly];
    builder = [builder setAutoDetermineTrackingAuthorizationTimeoutWithSecond:trackingAuthorizeTimeoutSeconds];
    builder = [builder setTrackInSessionLifecycleEventEnabled:trackInSessionLifeCycleEventEnabled];
    builder = [builder setPauseEventTransmitOnBackgroundEnabled:pauseEventTransmitOnBackgroundEnabled];
    builder = [builder setClearEventBufferOnInitializeEnabled:clearEventBufferOnInitializeEnabled];
    builder = [builder setSDKEnabled:sdkEnabled];
    builder = [builder setEventBufferCountLimit:eventBufferCountLimitInGibibyte];
    builder = [builder setEventBufferSizeLimitWithGibibyte:eventBufferSizeLimitInGibibyte];
    builder = [builder setEventTransmitIntervalWithSecond:eventTransmitIntervalSeconds];
    builder = [builder setCalculateSKAdNetworkByServerEnabled:calculateSKAdNetworkByServer];
    builder = [builder setOnInAppPurchaseReceived:^(AirbridgeInAppPurchase * _Nonnull inAppPurchase) {
        if (self.inAppPurchaseOnReceived == nil) { return; }
        self.inAppPurchaseOnReceived(inAppPurchase);
    }];
    builder = [builder setOnAttributionReceived:^(NSDictionary<NSString *,NSString *> * dictionary) {
    if (self.attributionOnReceived == nil) { return; }
        self.attributionOnReceived([AUConvert stringFromDictionary:dictionary]);
    }];
    builder = [builder
     setSDKSignatureWithId:sdkSignatureSecretID
     secret:sdkSignatureSecret
    ];
    
    builder = [builder setSDKAttributes: @{
        @"wrapperName": @"airbridge-unity-sdk-restricted",
        @"wrapperVersion": @"4.9.2"
    }];
    
    builder = [builder setSDKWrapperOption: @{
        @"isHandleAirbridgeDeeplinkOnly": @(isHandleAirbridgeDeeplinkOnly)
    }];
    
    if (trackingBlocklists.count != 0) {
        builder = [builder setTrackingBlocklist:trackingBlocklists];
    }
    
    [Airbridge initializeSDKWithOption:[builder build]];
    [Airbridge handleDeferredDeeplinkOnSuccess:^(NSURL * _Nullable deeplinkURL) {
        if (deeplinkURL == nil) { return; }
        if (AirbridgeUnity.sharedInstance.deeplinkOnReceived == nil) { 
             AirbridgeUnity.sharedInstance.initializeBeforeDeeplinkString = deeplinkURL.absoluteString;
             return; 
        }
        else { 
            AirbridgeUnity.sharedInstance.deeplinkOnReceived(deeplinkURL.absoluteString);
        }
    }];
}

@end

