//
//  AUOptionBuilder.m
//  AirbridgeUnity
//
//  Created by mjgu on 2026/09/17.
//  Copyright © 2026 ab180. All rights reserved.
//

#import "AUOptionBuilder.h"

@implementation AUOptionBuilder

+ (AirbridgeOptionBuilder *)optionBuilderWithOptions:(NSDictionary<NSString *, id> *)options {
    NSString *(^text)(NSString *) = ^NSString *(NSString *key) {
        id value = options[key];
        return [value isKindOfClass:NSString.class] ? value : nil;
    };
    NSNumber *(^number)(NSString *) = ^NSNumber *(NSString *key) {
        id value = options[key];
        return [value isKindOfClass:NSNumber.class] ? value : nil;
    };
    NSArray *(^list)(NSString *) = ^NSArray *(NSString *key) {
        id value = options[key];
        return [value isKindOfClass:NSArray.class] ? value : nil;
    };
    // Options use the QA Library export keys and types; no legacy aliases.
    if (text(@"appName").length == 0 || text(@"appToken").length == 0) { return nil; }

    AirbridgeOptionBuilder *builder = [[AirbridgeOptionBuilder alloc]
        initWithName:text(@"appName")
        token:text(@"appToken")
    ];
    NSString *serverDomain = text(@"serverDomain");
    SEL serverDomainSelector = NSSelectorFromString(@"setServerDomain:");
    if (serverDomain.length != 0 && [builder respondsToSelector:serverDomainSelector]) {
        typedef AirbridgeOptionBuilder *(*SetServerDomain)(id, SEL, NSString *);
        SetServerDomain setServerDomain = (SetServerDomain)[builder methodForSelector:serverDomainSelector];
        setServerDomain(builder, serverDomainSelector, serverDomain);
    } else if (serverDomain.length != 0) {
        NSLog(@"(Airbridge QA) This iOS SDK does not support serverDomain: %@", serverDomain);
    }

    NSDictionary<NSString *, NSNumber *> *logLevels = @{
        @"debug": @(AirbridgeLogLevelDebug),
        @"info": @(AirbridgeLogLevelInfo),
        @"warning": @(AirbridgeLogLevelWarning),
        @"error": @(AirbridgeLogLevelError),
        @"fault": @(AirbridgeLogLevelFault)
    };
    NSString *logLevelName = text(@"logLevel").lowercaseString;
    NSNumber *configuredLogLevel = number(@"logLevel");
    if (configuredLogLevel == nil && logLevelName != nil) {
        configuredLogLevel = logLevels[logLevelName];
    }
    if (configuredLogLevel != nil) {
        [builder setLogLevel:(AirbridgeLogLevel)configuredLogLevel.integerValue];
    }
    if (number(@"paginateLogEnabled") != nil) {
        [builder setPaginateLogEnabled:number(@"paginateLogEnabled").boolValue];
    }
    if (number(@"logSubsystemPrintedInBodyEnabled") != nil) {
        [builder setLogSubsystemPrintedInBodyEnabled:number(@"logSubsystemPrintedInBodyEnabled").boolValue];
    }
    if (number(@"autoStartTrackingEnabled") != nil) {
        [builder setAutoStartTrackingEnabled:number(@"autoStartTrackingEnabled").boolValue];
    }
    NSNumber *sessionTimeout = number(@"sessionTimeoutInSecond");
    if (sessionTimeout != nil) {
        [builder setSessionTimeoutWithSecond:sessionTimeout.doubleValue];
    }
    if (number(@"hashUserInformationEnabled") != nil) {
        [builder setHashUserInformationEnabled:number(@"hashUserInformationEnabled").boolValue];
    }
    if (number(@"trackAirbridgeDeeplinkOnlyEnabled") != nil) {
        [builder setTrackAirbridgeDeeplinkOnlyEnabled:number(@"trackAirbridgeDeeplinkOnlyEnabled").boolValue];
    }
    if (number(@"trackMetaDeferredAppLinkEnabled") != nil) {
        [builder setTrackMetaDeferredAppLinkEnabled:number(@"trackMetaDeferredAppLinkEnabled").boolValue];
    }
    if (number(@"clearEventBufferOnInitializeEnabled") != nil) {
        [builder setClearEventBufferOnInitializeEnabled:number(@"clearEventBufferOnInitializeEnabled").boolValue];
    }
    if (number(@"sdkEnabled") != nil) {
        [builder setSDKEnabled:number(@"sdkEnabled").boolValue];
    }
    if (number(@"eventBufferSizeLimitInGibibyte") != nil) {
        [builder setEventBufferSizeLimitWithGibibyte:number(@"eventBufferSizeLimitInGibibyte").doubleValue];
    }
    if (number(@"eventBufferCountLimit") != nil) {
        [builder setEventBufferCountLimit:number(@"eventBufferCountLimit").integerValue];
    }
    if (number(@"eventTransmitIntervalInSecond") != nil) {
        [builder setEventTransmitIntervalWithSecond:number(@"eventTransmitIntervalInSecond").doubleValue];
    }
    if (number(@"autoDetermineTrackingAuthorizationTimeoutInSecond") != nil) {
        [builder setAutoDetermineTrackingAuthorizationTimeoutWithSecond:number(@"autoDetermineTrackingAuthorizationTimeoutInSecond").doubleValue];
    }
    if (number(@"pauseEventTransmitOnBackgroundEnabled") != nil) {
        [builder setPauseEventTransmitOnBackgroundEnabled:number(@"pauseEventTransmitOnBackgroundEnabled").boolValue];
    }
    if (number(@"collectTCFDataEnabled") != nil) {
        [builder setCollectTCFDataEnabled:number(@"collectTCFDataEnabled").boolValue];
    }
    if (number(@"calculateSKAdNetworkByServerEnabled") != nil) {
        [builder setCalculateSKAdNetworkByServerEnabled:number(@"calculateSKAdNetworkByServerEnabled").boolValue];
    }
    if (number(@"isMigrateUnisolatedStorageEnabled") != nil) {
        [builder setInternalOptionWithKey:@"migrateUnisolatedStorageEnabled" value:number(@"isMigrateUnisolatedStorageEnabled")];
    }

    NSMutableArray<NSString *> *customDomains = [NSMutableArray array];
    for (id domain in list(@"trackingLinkCustomDomains")) {
        if (![domain isKindOfClass:NSString.class]) { continue; }
        NSString *trimmed = [domain stringByTrimmingCharactersInSet:NSCharacterSet.whitespaceAndNewlineCharacterSet];
        if (trimmed.length != 0) { [customDomains addObject:trimmed]; }
    }
    if (list(@"trackingLinkCustomDomains") != nil) { [builder setTrackingLinkCustomDomains:customDomains]; }

    NSMutableArray<AirbridgeTrackingBlocklist *> *blocklist = [NSMutableArray array];
    for (id entry in list(@"trackingBlocklist")) {
        if (![entry isKindOfClass:NSString.class]) { continue; }
        NSString *value = [entry stringByTrimmingCharactersInSet:NSCharacterSet.whitespaceAndNewlineCharacterSet].localizedLowercaseString;
        if ([value isEqualToString:@"idfa"]) {
            [blocklist addObject:AirbridgeTrackingBlocklist.idfa];
        } else if ([value isEqualToString:@"idfv"]) {
            [blocklist addObject:AirbridgeTrackingBlocklist.idfv];
        } else if ([value isEqualToString:@"skadnetwork"]) {
            [blocklist addObject:AirbridgeTrackingBlocklist.skAdNetwork];
        }
    }
    if (list(@"trackingBlocklist") != nil) { [builder setTrackingBlocklist:blocklist]; }

    NSString *environment = text(@"inAppPurchaseEnvironment").lowercaseString;
    if ([environment isEqualToString:@"sandbox"]) {
        [builder setInAppPurchaseEnvironment:AirbridgeInAppPurchaseEnvironmentSandbox];
    } else if ([environment isEqualToString:@"production"]) {
        [builder setInAppPurchaseEnvironment:AirbridgeInAppPurchaseEnvironmentProduction];
    }

    NSString *appGroup = text(@"appGroup");
    if ([appGroup hasPrefix:@"group."]) { [builder setAppGroup:appGroup]; }
    NSString *signatureID = text(@"sdkSignatureID");
    NSString *signatureKey = text(@"sdkSignatureSecret");
    if (signatureID.length != 0 && signatureKey.length != 0) {
        [builder setSDKSignatureWithId:signatureID secret:signatureKey];
    }

    if (text(@"sdkDevelopmentPlatform") != nil) {
        [builder setSDKDevelopmentPlatform:text(@"sdkDevelopmentPlatform")];
    }
    [builder setSDKAttributes:@{
        @"wrapperName": @"airbridge-unity-sdk",
        @"wrapperVersion": @"4.11.0"
    }];
    if (number(@"isHandleAirbridgeDeeplinkOnly")) {
        [builder setSDKWrapperOption:@{
            @"isHandleAirbridgeDeeplinkOnly": number(@"isHandleAirbridgeDeeplinkOnly")
        }];
    }

    return builder;
}

@end
