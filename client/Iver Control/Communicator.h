//
//  Controller.h
//  Iver Control
//
//  Created by Yukun Lin on 12/29/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Communicator : NSObject<NSStreamDelegate>

@property int Port;
@property NSString* IP;
@property int Trim;
@property int Throttle;
@property int Rudder;
@property double Heading;
@property double Speed;
@property double Pitch;
@property double Roll;
@property (weak) UIViewController* View;

-(id) initWithAddress:(NSString*)ip port:(int)port andDelegateView:(UIViewController*)view;
-(void) sendMessage;
-(void) openStream;
-(void) closeStream;

@end
