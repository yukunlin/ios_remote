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
@property (weak, nonatomic) IBOutlet UIBarButtonItem *btnConnect;
@end

@implementation ConnectViewController

-(void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    ControllerViewController *vc = (ControllerViewController*) [[segue destinationViewController] topViewController];
    self.con = [[Communicator alloc] initWithAddress:[self.address text] port:[[self.port text] intValue] andDelegateView:vc];
    vc.con = self.con;
    [self.address resignFirstResponder];
}

- (IBAction)unwindToConnect:(UIStoryboardSegue *)segue
{
    ControllerViewController *vc = [segue sourceViewController];
    [vc CleanUp];
    [self.address becomeFirstResponder];
}

-(void) checkInput
{
    if (self.address.text.length > 0 && self.port.text.length > 0)
        self.btnConnect.enabled = YES;
    else
        self.btnConnect.enabled = NO;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    [self.address becomeFirstResponder];
    [self.address addTarget:self action:@selector(checkInput) forControlEvents:UIControlEventEditingChanged];
    [self.port addTarget:self action:@selector(checkInput) forControlEvents:UIControlEventEditingChanged];
    self.btnConnect.enabled = NO;
}

@end
