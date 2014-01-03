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
    
    // Draw heading indicator line
    CGFloat xBottom = cos(1.5 * M_PI) * radius * 1 + centerX;
    CGFloat yBottom = sin(1.5 * M_PI) * radius * 1 + centerY;
    CGFloat xTop = cos(1.5 * M_PI) * radius * 0.65 + centerX;
    CGFloat yTop = sin(1.5 * M_PI) * radius * 0.65 + centerY;
    
    CGContextMoveToPoint(context, xBottom, yBottom);
    CGContextAddLineToPoint(context, xTop, yTop);
    CGContextSetLineWidth(context, 2.7);
    CGContextStrokePath(context);
    
    // Draw Center Circle
    CGContextAddEllipseInRect(context,
                              CGRectMake(centerX - .25 * radius, centerY - .25 * radius, .5 * radius, .5 * radius));
    
    CGContextSetFillColorWithColor(context, [UIColor colorWithWhite:0.1 alpha:0.04].CGColor);
    CGContextFillPath(context);
    
    // Draw center cross
    CGContextMoveToPoint(context, centerX - .06 * radius, centerY);
    CGContextAddLineToPoint(context, centerX + .06 * radius, centerY);
    CGContextSetLineWidth(context, .7);
    CGContextStrokePath(context);
    
    CGContextMoveToPoint(context, centerX, centerY - .06 * radius);
    CGContextAddLineToPoint(context, centerX, centerY + .06 * radius);
    CGContextStrokePath(context);
}

- (double) limitValue:(double)value
{
    if (value > 20)
        return 20;
    else if (value < -20)
        return -20;
    else
        return value;
}

-(void) Translate:(double) pitch row:(double) row withRate:(double)r
{
    [UIView beginAnimations:nil context:NULL];
    [UIView setAnimationDuration:r];
    [UIView setAnimationCurve:UIViewAnimationCurveLinear];
    
    CGFloat radius = self.bounds.size.width/2;
    
    self.transform = CGAffineTransformMakeTranslation(
            [self limitValue:row] * (.3/20.0) * radius,
            [self limitValue:pitch] * (.3/20.0) * radius);
    
    [UIView commitAnimations];
}

@end
