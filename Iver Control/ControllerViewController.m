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
#import <QuartzCore/QuartzCore.h>

@interface ControllerViewController ()
{
    LargeSlider *trim;
    LargeSlider *throttle;
}
@property (weak, nonatomic) IBOutlet UIBarButtonItem *stop;
@property (strong, atomic) CMMotionManager *motionManager;
@property (weak, nonatomic) IBOutlet UILabel *lblTrim;
@property (weak, nonatomic) IBOutlet UILabel *lblThrottle;
@property (weak, nonatomic) IBOutlet UILabel *lblPitch;
@property (weak, nonatomic) IBOutlet UILabel *lblSpeed;
@property Compass* compass;
@end

@implementation ControllerViewController


- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Set up sliders
    NSInteger margin = -50;
    NSInteger height = 230;
    CGRect trimFrame = CGRectMake(margin, self.view.frame.size.width/2+10, height, 20);
    CGRect throttleFrame = CGRectMake(self.view.frame.size.height-(height+margin),self.view.frame.size.width/2+10, height, 20);

    trim = [[LargeSlider alloc] initWithFrame:trimFrame];
    [self initializeSlider:trim action:@selector(trimAction:)];
    
    throttle = [[LargeSlider alloc] initWithFrame:throttleFrame];
    [self initializeSlider:throttle action:@selector(throttleAction:)];
    
    // Set up accelerometer
    self.motionManager = [[CMMotionManager alloc] init];
    self.motionManager.deviceMotionUpdateInterval = 0.1;
    
    // Set up Compass
    NSInteger compassSize = 220;
    self.compass = [[Compass alloc] initWithFrame:
                    CGRectMake((self.view.frame.size.height-compassSize)/2,(self.view.frame.size.width-compassSize)/2+22,compassSize,compassSize)];
    [self.view addSubview:self.compass];
    
    // Start accelerometer and networking
    [self.con openStream];
    [self performSelectorInBackground:@selector(startCommLoop) withObject:nil];
    [self performSelectorInBackground:@selector(updateGUI) withObject:nil];
    [self.motionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue currentQueue]
                                            withHandler:^(CMDeviceMotion *motion, NSError *error)
    {
        if(error){
            NSLog(@"%@", error);
        }
        else
        {
            self.con.Rudder = (int) (128 - 3.5 * (motion.attitude.pitch) / (M_PI/2) *128);
        }
    }
    ];
}

- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    [self performSegueWithIdentifier:@"unwindToConnect" sender:self];
}

-(void) startCommLoop
{
    while (self.con.CommStart)
    {
        [NSThread sleepForTimeInterval:.2];
        [self.con sendMessage];
    }
    
    [self.motionManager stopDeviceMotionUpdates];
    [self.con closeStream];
}

-(void) updateGUI
{
    while (self.con.CommStart)
    {
        [NSThread sleepForTimeInterval:.4];
        dispatch_async(dispatch_get_main_queue(),
                       ^{
                           [self.compass Rotate:self.con.Heading * M_PI / -180 withRate:.4];
                           [self.compass Translate:self.con.Pitch row:self.con.Row withRate:.4];
                           self.lblPitch.text = [NSString stringWithFormat:@"%.1f", self.con.Pitch];
                           self.lblSpeed.text = [NSString stringWithFormat:@"%.1f", self.con.Speed];
                       });
    }
}


-(void) initializeSlider:(LargeSlider*) slider action:(SEL) action
{
    slider.maximumValue = 256;
    slider.minimumValue = 0;
    slider.value = 128;
    [slider addTarget:self action:action forControlEvents:UIControlEventValueChanged];
    [slider setBackgroundColor:[UIColor clearColor]];
    slider.continuous = YES;
    slider.transform = CGAffineTransformMakeRotation(-M_PI/2);
    [self.view addSubview:slider];
}

-(IBAction) trimAction:(id) sender{
    self.con.Trim = (int) trim.value;
    self.lblTrim.text = [NSString stringWithFormat:@"%d", (int) trim.value];
}

-(IBAction) throttleAction:(id) sender{
    self.con.Throttle = (int) throttle.value;
    self.lblThrottle.text = [NSString stringWithFormat:@"%d", (int) throttle.value];
}

-(IBAction) stopPressed:(id) sender{
    [throttle setValue:128 animated:0];
    self.con.Throttle = 128;
    self.lblThrottle.text = [NSString stringWithFormat:@"%d", (int) throttle.value];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
