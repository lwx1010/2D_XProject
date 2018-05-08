/***************************************
*** SDKDefaultController.m sdk基类
*** Author: luweixing
*** Mail: kinglucn@hotmail.com
*** Time: 2018-01-04 15:05:49
***************************************/

#include <sys/sysctl.h> 
#include <net/if.h> 
#include <net/if_dl.h> 

#import <ifaddrs.h>
#import <arpa/inet.h>
#import <SystemConfiguration/CaptiveNetwork.h>

#import <HWSDK/HWSDK.h>
#import "SDKDefaultController.h"
#import "SDKHWController.h"

#if defined(__cplusplus)
extern "C"{
#endif
    
    extern void UnitySendMessage(const char* gameObjectName, const char* methodName, const char* param);
    
#if defined(__cplusplus)
}
#endif

#define CALLBACK_GAMEOBJECT_NAME "(sdk_callback)"

@implementation SDKDefaultController

static SDKDefaultController* _instance = nil;

+(instancetype) shareInstance  
{  
    if (_instance == nil)
        _instance = [[SDKHWController alloc] init];
    
    return _instance;
}

-(id) init
{
    self = [super init];
    return self;
}

-(void) SendCallback:(const char*)method withParams:(NSDictionary *)params
{
    NSString* jsStr = nil;
    
    if (params)
    {
        NSError* error;
        NSData* data = [NSJSONSerialization dataWithJSONObject:params options:kNilOptions error:&error];
        
        if (data)
        {
            jsStr = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        }
    }
    
    if (jsStr)
    {
        UnitySendMessage(CALLBACK_GAMEOBJECT_NAME, method, [jsStr UTF8String]);
    }
    else
    {
        UnitySendMessage(CALLBACK_GAMEOBJECT_NAME, method, "");
    }
}

-(void) showAlertView:(NSString *)message
{
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"" message:message delegate:self cancelButtonTitle:@"确定" otherButtonTitles:nil, nil];
    [alert show];
}

@end
