//
//  LargeSlider.m
//  Iver Control
//
//  Created by Yukun Lin on 12/30/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import "LargeSlider.h"
#define THUMB_SIZE 10
#define EFFECTIVE_THUMB_SIZE 20

@implementation LargeSlider

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        // Initialization code
    }
    return self;
}

- (BOOL) pointInside:(CGPoint)point withEvent:(UIEvent*)event {
    CGRect bounds = self.bounds;
    bounds = CGRectInset(bounds, -10, -8);
    return CGRectContainsPoint(bounds, point);
}

- (BOOL) beginTrackingWithTouch:(UITouch*)touch withEvent:(UIEvent*)event {
    CGRect bounds = self.bounds;
    float thumbPercent = (self.value - self.minimumValue) / (self.maximumValue - self.minimumValue);
    float thumbPos = THUMB_SIZE + (thumbPercent * (bounds.size.width - (2 * THUMB_SIZE)));
    CGPoint touchPoint = [touch locationInView:self];
    return (touchPoint.x >= (thumbPos - EFFECTIVE_THUMB_SIZE) &&
            touchPoint.x <= (thumbPos + EFFECTIVE_THUMB_SIZE));
}
/*
// Only override drawRect: if you perform custom drawing.
// An empty implementation adversely affects performance during animation.
- (void)drawRect:(CGRect)rect
{
    // Drawing code
}
*/

@end
