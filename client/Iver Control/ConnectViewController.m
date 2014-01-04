//
//  ConnectViewController.m
//  Iver Control
//
//  Created by Yukun Lin on 12/28/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import "ConnectViewController.h"
#import "ControllerViewController.h"

@interface ConnectViewController ()

@property (weak, nonatomic) IBOutlet UITextField *address;
@property (weak, nonatomic) IBOutlet UITextField *port;


@end

@implementation ConnectViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

-(void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    ControllerViewController *vc = (ControllerViewController*) [[segue destinationViewController] topViewController];
    self.con = [[Communicator alloc] initWithAddress:[self.address text] port:[[self.port text] intValue] andDelegateView:vc];
    vc.con = self.con;
    [self.address resignFirstResponder];
}

- (IBAction)unwindToConnect:(UIStoryboardSegue *)segue
{
    self.con.CommStart = false;
    [self.con.Timer invalidate];
    [self.address becomeFirstResponder];
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    [self.address becomeFirstResponder];
}

@end
