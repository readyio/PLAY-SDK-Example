#import <Foundation/NSException.h>
#import <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>

@interface PermissionProviderHelper : NSObject {}

- (void) verifyVideoPermission:(NSString *)gameObject withCallback:(NSString *)callback;
- (void) verifyAudioPermission:(NSString *)gameObject withCallback:(NSString *)callback;

@end
