//
//  AirbridgeUnity.h
//  AirbridgeUnity
//
//  Created by WOF on 29/11/2019.
//

#import "AirbridgeUnity.h"

#import <Airbridge/Airbridge.h>

#import "AUConvert.h"
#import "AUOptionBuilder.h"
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
    NSMutableDictionary<NSString *, id> *options = [NSMutableDictionary dictionary];
    options[@"appName"] = appName;
    options[@"appToken"] = appToken;
    options[@"sdkSignatureID"] = sdkSignatureSecretID;
    options[@"sdkSignatureSecret"] = sdkSignatureSecret;
    options[@"logLevel"] = @(logLevel);
    options[@"sessionTimeoutInSecond"] = @(sessionTimeoutSeconds);
    options[@"autoStartTrackingEnabled"] = @(autoStartTrackingEnabled);
    options[@"hashUserInformationEnabled"] = @(userInfoHashEnabled);
    options[@"trackAirbridgeDeeplinkOnlyEnabled"] = @(trackAirbridgeLinkOnly);
    options[@"trackMetaDeferredAppLinkEnabled"] = @(facebookDeferredAppLinkEnabled);
    options[@"autoDetermineTrackingAuthorizationTimeoutInSecond"] = @(trackingAuthorizeTimeoutSeconds);
    options[@"pauseEventTransmitOnBackgroundEnabled"] = @(pauseEventTransmitOnBackgroundEnabled);
    options[@"clearEventBufferOnInitializeEnabled"] = @(clearEventBufferOnInitializeEnabled);
    options[@"sdkEnabled"] = @(sdkEnabled);
    options[@"eventBufferCountLimit"] = @(eventBufferCountLimitInGibibyte);
    options[@"eventBufferSizeLimitInGibibyte"] = @(eventBufferSizeLimitInGibibyte);
    options[@"eventTransmitIntervalInSecond"] = @(eventTransmitIntervalSeconds);
    options[@"isHandleAirbridgeDeeplinkOnly"] = @(isHandleAirbridgeDeeplinkOnly);
    options[@"inAppPurchaseEnvironment"] = inAppPurchaseEnvironment;
    options[@"collectTCFDataEnabled"] = @(collectTCFDataEnabled);
    options[@"trackingBlocklist"] = [trackingBlocklist componentsSeparatedByString:@","];
    options[@"calculateSKAdNetworkByServerEnabled"] = @(calculateSKAdNetworkByServer);
    options[@"sdkDevelopmentPlatform"] = @"unity";
    options[@"trackingLinkCustomDomains"] = [customDomain componentsSeparatedByCharactersInSet:
        NSCharacterSet.whitespaceAndNewlineCharacterSet];
    AirbridgeOptionBuilder *builder = [AUOptionBuilder optionBuilderWithOptions:options];
    if (builder == nil) { return; }
    builder = [builder setOnInAppPurchaseReceived:^(AirbridgeInAppPurchase * _Nonnull inAppPurchase) {
        if (self.inAppPurchaseOnReceived == nil) { return; }
        self.inAppPurchaseOnReceived(inAppPurchase);
    }];
    builder = [builder setOnAttributionReceived:^(NSDictionary<NSString *,NSString *> * dictionary) {
        if (self.attributionOnReceived == nil) { return; }
        self.attributionOnReceived([AUConvert stringFromDictionary:dictionary]);
    }];

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
