//
//  StaticPlate.m
//  Iver Control
//
//  Created by Yukun Lin on 1/3/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import "StaticPlate.h"

@implementation StaticPlate

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
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

    
    CGContextMoveToPoint(context, centerX - .35 * radius, centerY);
    CGContextAddLineToPoint(context, centerX + .35 * radius, centerY);
    CGContextSetLineWidth(context, .7);
    CGContextStrokePath(context);
    
    CGContextMoveToPoint(context, centerX, centerY - .35 * radius);
    CGContextAddLineToPoint(context, centerX, centerY + .35 * radius);
    CGContextStrokePath(context);
}

@end
