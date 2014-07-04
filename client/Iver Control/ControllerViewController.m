//
//  MainViewController.m
//  Iver Control
//
//  Created by Yukun Lin on 12/27/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import "ControllerViewController.h"
#import <CoreMotion/CoreMotion.h>
#import "LargeSlider.h"
#import "Compass.h"

@interface ControllerViewController ()

@property LargeSlider *trim;
@property LargeSlider *throttle;
@property (weak, nonatomic) IBOutlet UIBarButtonItem *stop;
@property (strong, atomic) CMMotionManager *motionManager;
@property (weak, nonatomic) IBOutlet UILabel *lblTrim;
@property (weak, nonatomic) IBOutlet UILabel *lblThrottle;
@property (weak, nonatomic) IBOutlet UILabel *lblRudder;
@property (weak, nonatomic) IBOutlet UILabel *lblSpeed;
@property Compass* compass;
@property NSTimer* commTimer;
@property NSTimer* guiTimer;
@end

@implementation ControllerViewController

double period = 0.25;

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Set up sliders
    NSInteger margin = -60;
    NSInteger height = 230;
    CGRect trimFrame = CGRectMake(margin, self.view.frame.size.width/2+10, height, 20);
    CGRect throttleFrame = CGRectMake(self.view.frame.size.height-(height+margin),self.view.frame.size.width/2+10, height, 20);

    self.trim = [[LargeSlider alloc] initWithFrame:trimFrame];
    [self initializeSlider:self.trim action:@selector(trimAction:)];
    
    self.throttle = [[LargeSlider alloc] initWithFrame:throttleFrame];
    [self initializeSlider:self.throttle action:@selector(throttleAction:)];
    
    // Set up accelerometer
    self.motionManager = [[CMMotionManager alloc] init];
    self.motionManager.deviceMotionUpdateInterval = 0.2;
    
    // Set up Compass
    NSInteger compassSize = 220;
    self.compass = [[Compass alloc] initWithFrame:
                    CGRectMake((self.view.frame.size.height-compassSize)/2,(self.view.frame.size.width-compassSize)/2+22,compassSize,compassSize)];
    [self.view addSubview:self.compass];
    
    // Start accelerometer and networking
    [self.con openStream];
    self.commTimer = [NSTimer scheduledTimerWithTimeInterval:period target:self selector:@selector(sendMessage) userInfo:nil repeats:YES];
    self.guiTimer = [NSTimer scheduledTimerWithTimeInterval:2 * period target:self selector:@selector(updateGUI) userInfo:nil repeats:YES];
    
    [self.motionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue currentQueue]
                                            withHandler:^(CMDeviceMotion *motion, NSError *error)
    {
        if(error)
            NSLog(@"%@", error);
        else
        {
            self.con.Rudder = (int) MIN( MAX( (128 + 3.5 * (motion.attitude.pitch) / (M_PI/2) *128), 0), 255);
            self.lblRudder.text = [NSString stringWithFormat:@"%d", self.con.Rudder];
        }
    }
    ];
}

- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if (buttonIndex == 0)
        [self performSegueWithIdentifier:@"unwindToConnect" sender:self];
    else
    {
        [self.con closeStream];
        [self.con openStream];
    }
}

-(void) sendMessage
{
    [self.con sendMessage];
}
    

-(void) updateGUI
{
    [self.compass rotate:self.con.Heading * M_PI / -180 withRate:2 * period];
    [self.compass translate:self.con.Pitch roll:self.con.Roll withRate:2 * period];
    self.lblSpeed.text = [NSString stringWithFormat:@"%.1f", self.con.Speed];
}

-(void) CleanUp
{
    [self.motionManager stopDeviceMotionUpdates];
    [self.con closeStream];
    [self.guiTimer invalidate];
    [self.commTimer invalidate];
}

-(void) initializeSlider:(LargeSlider*) slider action:(SEL) action
{
    slider.maximumValue = 255;
    slider.minimumValue = 0;
    slider.value = 128;
    [slider addTarget:self action:action forControlEvents:UIControlEventValueChanged];
    [slider setBackgroundColor:[UIColor clearColor]];
    slider.continuous = YES;
    slider.transform = CGAffineTransformMakeRotation(-M_PI/2);
    [self.view addSubview:slider];
}

-(IBAction) trimAction:(id) sender{
    self.con.Trim = (int) self.trim.value;
    self.lblTrim.text = [NSString stringWithFormat:@"%d", (int) self.trim.value];
}

-(IBAction) throttleAction:(id) sender{
    self.con.Throttle = (int) self.throttle.value;
    self.lblThrottle.text = [NSString stringWithFormat:@"%d", (int) self.throttle.value];
}

-(IBAction) stopPressed:(id) sender{
    [self.throttle setValue:128 animated:0];
    self.con.Throttle = 128;
    self.lblThrottle.text = [NSString stringWithFormat:@"%d", (int) self.throttle.value];
}


@end
