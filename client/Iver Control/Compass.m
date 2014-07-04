//
//  Compass.m
//  Iver Control
//
//  Created by Yukun Lin on 1/2/14.
//  Copyright (c) 2014 Yukun Lin. All rights reserved.
//

#import "Compass.h"
#import "RotatingPlate.h"
#import "StaticPlate.h"
#import "TranslationalPlate.h"

@interface Compass()
@property RotatingPlate *plate;
@property StaticPlate *sPlate;
@property TranslationalPlate *tPlate;
@end

@implementation Compass

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self)
    {
        self.backgroundColor = [UIColor clearColor];

        self.plate = [[RotatingPlate alloc] initWithFrame:CGRectMake(0, 0, frame.size.width, frame.size.height)];
        self.sPlate = [[StaticPlate alloc] initWithFrame:CGRectMake(0, 0, frame.size.width, frame.size.height)];
        self.tPlate = [[TranslationalPlate alloc] initWithFrame:CGRectMake(0, 0, frame.size.width, frame.size.height)];
        
        [self addSubview:self.sPlate];
        [self addSubview:self.plate];
        [self addSubview:self.tPlate];
    }
    return self;
}

-(void) rotate:(double) angle withRate:(double)r;
{
    [self.plate rotate:angle withRate:r];
}

-(void) translate:(double) pitch roll:(double) roll withRate:(double)r
{
    [self.tPlate translate:pitch roll:roll withRate:r];
}

@end
