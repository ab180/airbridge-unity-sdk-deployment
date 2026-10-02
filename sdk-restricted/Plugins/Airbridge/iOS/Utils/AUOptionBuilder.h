//
//  AUOptionBuilder.h
//  AirbridgeUnity
//
//  Created by mjgu on 2026/09/17.
//  Copyright © 2026 ab180. All rights reserved.
//

#import <Airbridge/Airbridge.h>

NS_ASSUME_NONNULL_BEGIN

// Internal dictionary-to-option conversion shared by the SDK and QA app.
@interface AUOptionBuilder : NSObject

// Missing, null, and incorrectly typed option values are skipped.
+ (nullable AirbridgeOptionBuilder *)optionBuilderWithOptions:(NSDictionary<NSString *, id> *)options;

@end

NS_ASSUME_NONNULL_END
