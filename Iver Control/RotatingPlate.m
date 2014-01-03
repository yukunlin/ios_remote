//
//  CompassPlate.m
//  Iver Control
//
//  Created by Yukun Lin on 1/2/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import "RotatingPlate.h"

@interface RotatingPlate()
@property NSMutableArray * compassMarkers;
@property NSMutableArray * headingMarkers;
@end

@implementation RotatingPlate

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        self.backgroundColor = [UIColor clearColor];
        self.compassMarkers = [[NSMutableArray alloc] init];
        self.headingMarkers = [[NSMutableArray alloc] init];
    }
    return self;
}

- (void) initializeMarker:(UILabel*) label withText:(NSString*)dir ofSize:(int)size andBold:(BOOL)bold
{
    label.backgroundColor = [UIColor clearColor];
    label.textColor = [UIColor blackColor];
    label.text = dir;
    if (bold)
        label.font = [UIFont boldSystemFontOfSize:size];
    else
        label.font = [UIFont systemFontOfSize:size];
    label.textAlignment = NSTextAlignmentCenter;
}

-(void) Rotate:(double) angle withRate:(double)r
{
    
    [UIView beginAnimations:nil context:NULL];
    [UIView setAnimationDuration:r];
    [UIView setAnimationCurve:UIViewAnimationCurveLinear];
     
    self.transform = CGAffineTransformMakeRotation(angle);
    
    for (id object in self.compassMarkers)
       ((UILabel*) object).transform = CGAffineTransformMakeRotation(-angle);
    
    for (id object in self.headingMarkers)
        ((UILabel*) object).transform = CGAffineTransformMakeRotation(-angle);
    
    [UIView commitAnimations];
}

- (void)drawRect:(CGRect)rect
{
    CGRect frame = self.bounds;
    CGContextRef context = UIGraphicsGetCurrentContext();
    CGContextSetStrokeColorWithColor(context, [UIColor blackColor].CGColor);
    
    CGFloat centerX = frame.size.width/2;
    CGFloat centerY = frame.size.height/2;
    CGFloat radius = centerX;
        
    int numTicks = 72;
    NSArray * direction = @[@"E", @"S", @"W", @"N"];
    
    for (int i = 0; i < numTicks; i++)
    {
        // Draw Line markers
        CGFloat xInner = cos(i/72.0 * 2 * M_PI) * radius * 0.65 + centerX;
        CGFloat yInner = sin(i/72.0 * 2 * M_PI) * radius * 0.65 + centerY;
        CGFloat xOuter = cos(i/72.0 * 2 * M_PI) * radius * 0.75 + centerX;
        CGFloat yOuter = sin(i/72.0 * 2 * M_PI) * radius * 0.75 + centerY;
        
        CGContextMoveToPoint(context, xInner, yInner);
        CGContextAddLineToPoint(context, xOuter, yOuter);
        
        if (i % 6 == 0)
            CGContextSetLineWidth(context, 2.5);
        else
            CGContextSetLineWidth(context, 1.0);
        
        CGContextStrokePath(context);
        
        // Draw N, S, E, W labels
        int lblDirectionSize = 20;
        if (i % 18 == 0)
        {
            CGFloat xIInner = cos(i/72.0 * 2 * M_PI) * radius * 0.5 + centerX;
            CGFloat yIInner = sin(i/72.0 * 2 * M_PI) * radius * 0.5 + centerY;
            UILabel *lblDirection =
                [[UILabel alloc]
                initWithFrame:CGRectMake(xIInner-lblDirectionSize/2, yIInner-lblDirectionSize/2, lblDirectionSize, lblDirectionSize)];
            
            [self initializeMarker:lblDirection withText:direction[i/18] ofSize:16 andBold:YES];
            [self.compassMarkers addObject:lblDirection];
            [self addSubview:lblDirection];
        }
        
        // Draw heading labels
        int lblHeadingSize = 20;
        if (i % 6 ==0)
        {
            CGFloat xOOuter = cos(i/72.0 * 2 * M_PI) * radius * .9 + centerX;
            CGFloat yOOuter = sin(i/72.0 * 2 * M_PI) * radius * .9 + centerY;
            UILabel *lblHeading =
            [[UILabel alloc]
             initWithFrame:CGRectMake(xOOuter-lblHeadingSize/2, yOOuter-lblHeadingSize/2, lblHeadingSize, lblHeadingSize)];
            
            [self initializeMarker:lblHeading withText:[NSString stringWithFormat:@"%d", (90+(i/6)*30)%360] ofSize:11 andBold:NO];
            [self.headingMarkers addObject:lblHeading];
            [self addSubview:lblHeading];
        }
    }
    
    // Draw red triangle
    CGFloat size = 4.1;
    CGFloat xTriTip = cos(1.5 * M_PI) * radius * 0.85 + centerX;
    CGFloat yTriTip = sin(1.5 * M_PI) * radius * 0.85 + centerY;
    CGFloat xTriLeft = xTriTip + size;
    CGFloat yTriLeft = yTriTip + size / tan(M_PI / 6);
    CGFloat xTriRight = xTriTip - size;
    CGFloat yTriRight = yTriTip + size / tan(M_PI / 6);
    
    CGContextMoveToPoint(context, xTriTip, yTriTip);
    CGContextAddLineToPoint(context, xTriLeft, yTriLeft);
    CGContextAddLineToPoint(context, xTriRight, yTriRight);
    CGContextSetFillColorWithColor(context,[UIColor redColor].CGColor);
    CGContextFillPath(context);

}


@end
