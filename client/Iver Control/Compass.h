//
//  Compass.h
//  Iver Control
//
//  Created by Yukun Lin on 1/2/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface Compass : UIView
-(void) rotate:(double) angle withRate:(double)r;
-(void) translate:(double) pitch row:(double) row withRate:(double)r;

@end
