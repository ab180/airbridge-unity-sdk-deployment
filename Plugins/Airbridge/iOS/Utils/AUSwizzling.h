//
//  AUSwizzling.h
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <objc/runtime.h>

NS_ASSUME_NONNULL_BEGIN

@interface AUSwizzling : NSObject

// Maps destination selector names to replacement selector names on sourceClass.
// Installs one selector set per target class; subsequent calls do nothing.
+ (void)swizzleClassIfNeeded:(Class)targetClass
                sourceClass:(Class)sourceClass
                  selectors:(NSDictionary<NSString *, NSString *> *)selectors;

+ (IMP _Nullable)previousImplementationForSelector:(SEL)selector object:(id)object;

+ (void)invokePreviousImplementationForSelector:(SEL)selector
                                        object:(id)object
                                    invocation:(void (^)(IMP implementation, SEL selector))invocation;

@end

NS_ASSUME_NONNULL_END
