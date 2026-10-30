//
//  AUSwizzling.m
//  AirbridgeUnity
//
//  Copyright © 2026 ab180. All rights reserved.
//

#import "AUSwizzling.h"

static char kAUPreviousImplementationsKey;

@implementation AUSwizzling

+ (void)swizzleClassIfNeeded:(Class)targetClass
                sourceClass:(Class)sourceClass
                  selectors:(NSDictionary<NSString *, NSString *> *)selectors {
    if (objc_getAssociatedObject(targetClass, &kAUPreviousImplementationsKey)) { return; }

    NSMutableDictionary<NSString *, NSValue *> *previousImplementationsBySelector =
        [[NSMutableDictionary alloc] init];

    for (NSString *destinationSelectorName in selectors) {
        SEL destinationSelector = NSSelectorFromString(destinationSelectorName);
        SEL sourceSelector = NSSelectorFromString(selectors[destinationSelectorName]);

        [self addMethodWithDestinationSelector:destinationSelector
                               sourceSelector:sourceSelector
                                    fromClass:sourceClass
                                      toClass:targetClass
            previousImplementationsBySelector:previousImplementationsBySelector];
    }

    objc_setAssociatedObject(targetClass, &kAUPreviousImplementationsKey,
                             [previousImplementationsBySelector copy], OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

+ (IMP)previousImplementationForSelector:(SEL)selector object:(id)object {
    for (Class targetClass = object_getClass(object);
         targetClass; targetClass = class_getSuperclass(targetClass)) {
        NSDictionary<NSString *, NSValue *> *implementations =
            objc_getAssociatedObject(targetClass, &kAUPreviousImplementationsKey);
        NSValue *value = implementations[NSStringFromSelector(selector)];
        if (!value) { continue; }

        IMP implementation = NULL;
        [value getValue:&implementation];
        return implementation;
    }
    return NULL;
}

+ (void)invokePreviousImplementationForSelector:(SEL)selector
                                        object:(id)object
                                    invocation:(void (^)(IMP implementation, SEL selector))invocation {
    IMP implementation = [self previousImplementationForSelector:selector object:object];
    if (!implementation) { return; }

    invocation(implementation, selector);
}

+ (void)addMethodWithDestinationSelector:(SEL)destinationSelector
                         sourceSelector:(SEL)sourceSelector
                              fromClass:(Class)sourceClass
                                toClass:(Class)targetClass
      previousImplementationsBySelector:(NSMutableDictionary<NSString *, NSValue *> *)previousImplementationsBySelector {
    Method sourceMethod = class_getInstanceMethod(sourceClass, sourceSelector);
    if (!sourceMethod) { return; }

    IMP sourceImplementation = method_getImplementation(sourceMethod);
    const char *types = method_getTypeEncoding(sourceMethod);

    Method previousMethod = class_getInstanceMethod(targetClass, destinationSelector);
    IMP previousImplementation = previousMethod ? method_getImplementation(previousMethod) : NULL;

    // An inherited hook already forwards through the superclass's saved implementation.
    if (previousImplementation == sourceImplementation) { return; }
    if (previousImplementation) {
        previousImplementationsBySelector[NSStringFromSelector(destinationSelector)] =
            [NSValue valueWithBytes:&previousImplementation objCType:@encode(IMP)];
    }

    class_replaceMethod(targetClass, destinationSelector, sourceImplementation, types);
}

@end
