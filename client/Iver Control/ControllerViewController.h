//
//  MainViewController.h
//  Iver Control
//
//  Created by Yukun Lin on 12/27/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Communicator.h"

@interface ControllerViewController : UIViewController<UIAlertViewDelegate>
@property Communicator* con;

@end
