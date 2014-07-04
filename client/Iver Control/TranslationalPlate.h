//
//  TranslationalPlate.h
//  Iver Control
//
//  Created by Yukun Lin on 1/3/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface TranslationalPlate : UIView
-(void) translate:(double) pitch roll:(double) roll withRate:(double)r;

@end
