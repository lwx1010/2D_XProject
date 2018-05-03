/***************************************
*** iOSHelper.h
*** Author: luweixing
*** Mail: kinglucn@hotmail.com
*** Time: 2018-02-06 12:22:59
***************************************/

@interface iOSHelper : NSObject

extern "C"
{
	const char* getIPv6(const char* mHost);
}

@end