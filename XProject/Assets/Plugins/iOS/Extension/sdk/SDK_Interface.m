/***************************************
*** SDK_Interface.m (unity - iphone)
*** Author: luweixing
*** Mail: kinglucn@hotmail.com
*** Time: 2018-01-03 12:58:53
***************************************/

#import <Foundation/Foundation.h>
#import "SDKDefaultController.h"

void SDK_Setup()
{
    [[SDKDefaultController shareInstance] Setup];
}

void SDK_Login()
{
    [[SDKDefaultController shareInstance] Login];
}

bool SDK_Logout()
{
    [[SDKDefaultController shareInstance] Logout];
    
    return false;
}

void SDK_UserCenter()
{
    [[SDKDefaultController shareInstance] UserCenter];
}

bool SDK_SDKExit()
{
    return false;
}

void SDK_Pay(const char* payParams)
{
    NSString* strParams = [[NSString alloc] initWithUTF8String:payParams];
    [[SDKDefaultController shareInstance] OrderAndPay:strParams];
}

bool SDK_IsSupportExit()
{
    return false;
}

bool SDK_IsSupportLogout()
{
    return true;
}

void SDK_GetSDKInfo()
{
	[[SDKDefaultController shareInstance] GetSDKInfo];
}
