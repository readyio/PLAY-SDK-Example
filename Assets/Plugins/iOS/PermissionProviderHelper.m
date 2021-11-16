#import "PermissionProviderHelper.h"

@implementation PermissionProviderHelper
- (void) verifyVideoPermission:(NSString *)NSGameObject withCallback:(NSString *)NSCallback
{
    // if (iOS >= 7) ask for camera access;
    if ([AVCaptureDevice respondsToSelector:@selector(requestAccessForMediaType:completionHandler:)])
    {
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted)
        {
            
            if (granted == YES) {
                UnitySendMessage(([NSGameObject cStringUsingEncoding:NSUTF8StringEncoding]), ([NSCallback cStringUsingEncoding:NSUTF8StringEncoding]), "true");
            }
            else
            {
                UnitySendMessage(([NSGameObject cStringUsingEncoding:NSUTF8StringEncoding]), ([NSCallback cStringUsingEncoding:NSUTF8StringEncoding]), "false");
            }
        }];
    }
    // if (iOS < 7) camera access is always permitted.
    else 
    {
        UnitySendMessage(([NSGameObject cStringUsingEncoding:NSUTF8StringEncoding]), ([NSCallback cStringUsingEncoding:NSUTF8StringEncoding]), "true");
    }
}

- (void) verifyAudioPermission:(NSString *)NSGameObject withCallback:(NSString *)NSCallback
{
    [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
        if (granted == YES) {
            UnitySendMessage(([NSGameObject cStringUsingEncoding:NSUTF8StringEncoding]), ([NSCallback cStringUsingEncoding:NSUTF8StringEncoding]), "true");
        }
        else
        {
            UnitySendMessage(([NSGameObject cStringUsingEncoding:NSUTF8StringEncoding]), ([NSCallback cStringUsingEncoding:NSUTF8StringEncoding]), "false");
        }
    }];
}
@end
