//
//  TranslationalPlate.m
//  Iver Control
//
//  Created by Yukun Lin on 1/3/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import "TranslationalPlate.h"

@implementation TranslationalPlate

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self)
    {
        self.backgroundColor = [UIColor clearColor];
    }
    return self;
}

- (void)drawRect:(CGRect)rect
{
    CGRect frame = self.bounds;
    CGContextRef context = UIGraphicsGetCurrentContext();
    CGContextSetStrokeColorWithColor(context, [UIColor blackColor].CGColor);
    
    CGFloat centerX = frame.size.width/2;
    CGFloat centerY = frame.size.height/2;
    CGFloat radius = centerX;
    
    // Draw Center Circle
    CGContextAddEllipseInRect(context,
                              CGRectMake(centerX - .17 * radius, centerY - .17 * radius, .34 * radius, .34 * radius));
    
    CGContextSetFillColorWithColor(context, [UIColor colorWithWhite:0.1 alpha:0.04].CGColor);
    CGContextFillPath(context);
    
    // Draw center cross
    CGContextMoveToPoint(context, centerX - .05 * radius, centerY);
    CGContextAddLineToPoint(context, centerX + .05 * radius, centerY);
    CGContextSetLineWidth(context, .7);
    CGContextStrokePath(context);
    
    CGContextMoveToPoint(context, centerX, centerY - .05 * radius);
    CGContextAddLineToPoint(context, centerX, centerY + .05 * radius);
    CGContextStrokePath(context);
}

- (double) limitValue:(double)value withRange:(double) range
{
    return MAX(MIN(value, range), -range);
}

-(void) translate:(double) pitch roll:(double) roll withRate:(double)r
{
    CGFloat radius = self.bounds.size.width/2;
    [UIView animateWithDuration:r delay:0 options: UIViewAnimationOptionCurveLinear animations:^{
        self.transform = CGAffineTransformMakeTranslation(
                                                          [self limitValue:roll withRange:45.0] * -(.25/45.0) * radius,
                                                          [self limitValue:pitch withRange:15.0] * -(.25/15.0) * radius);
    }completion:nil];
}

@end