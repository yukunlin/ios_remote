//
//  Controller.m
//  Iver Control
//
//  Created by Yukun Lin on 12/29/13.
//  Copyright (c) 2013 Yukun Lin. All rights reserved.
//

#import "Communicator.h"

@implementation Communicator
{
    NSInputStream* inputStream;
    NSOutputStream* outputStream;
}

-(id) initWithAddress:(NSString*)ip port:(int)port andDelegateView:(UIViewController*)view
{
    self.Port = port;
    self.IP = ip;
    self.Trim = self.Throttle = self.Rudder = 128;
    self.CommStart = true;
    
    self.Heading = self.Speed = self.Pitch = self.Row = 0;
    self.View = view;
    return self;
}

-(void) openStream
{
    CFReadStreamRef readStream;
    CFWriteStreamRef writeStream;
    
    CFStreamCreatePairWithSocketToHost(kCFAllocatorDefault, (__bridge CFStringRef)(self.IP), (UInt32) self.Port, &readStream, &writeStream);
    inputStream = (__bridge_transfer NSInputStream *)readStream;
    outputStream = (__bridge_transfer NSOutputStream *)writeStream;
    
    [inputStream setDelegate:self];
    [outputStream setDelegate:self];
    
    [inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    
    [inputStream open];
    [outputStream open];
    [self setTimeOut];
    
}

-(void) setTimeOut
{
    self.Timer = [NSTimer scheduledTimerWithTimeInterval:5.0
                                                  target:self
                                                selector:@selector(showAlert)
                                                userInfo:nil
                                                 repeats:NO];
}

-(void) showAlert
{
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Network Error"
                                                    message:@"Connection lost with Iver."
                                                   delegate:self.View
                                          cancelButtonTitle:@"OK"
                                          otherButtonTitles:nil];
    [alert show];
    [self.Timer invalidate];
}

-(void) closeStream
{
    [inputStream close];
    [outputStream close];
    [inputStream setDelegate:nil];
    [outputStream setDelegate:nil];
    inputStream = nil;
    outputStream = nil;
}

-(void) sendMessage
{
    NSString* message = [NSString stringWithFormat:@"%d,%d,%d\r\n", self.Trim, self.Throttle, self.Rudder];
    NSData* data = [[NSData alloc] initWithData:[message dataUsingEncoding:NSASCIIStringEncoding]];
    
    [outputStream write:[data bytes] maxLength:[data length]];
}

- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)eventCode
{
    switch(eventCode)
    {
        case NSStreamEventHasBytesAvailable:
        {				
            uint8_t buf[1024];
            unsigned int len = 0;
			
            @try
            {
                len = [inputStream read:buf maxLength:1024];
                
                if(len > 0)
                {
                    NSMutableData* data=[[NSMutableData alloc] initWithLength:0];
                    
                    [data appendBytes: (const void *)buf length:len];
                        
                    NSString *s = [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
                    NSArray *split = [s componentsSeparatedByString:@","];
                    
                    self.Heading = [[split objectAtIndex:0] doubleValue];
                    self.Speed = [[split objectAtIndex:1] doubleValue];
                    self.Pitch = [[split objectAtIndex:2] doubleValue];
                    self.Row = [[split objectAtIndex:3] doubleValue];
                    
                    NSLog(@"Heading: %f, Speed: %f, Pitch: %f, Row: %f", self.Heading, self.Speed, self.Pitch, self.Row);
                    
                    // Reset timeout Timer
                    [self.Timer invalidate];
                    [self setTimeOut];
                }
            } @catch (NSException *exception) {
                [self showAlert];	
            }
        }
        break;
    }
}

@end