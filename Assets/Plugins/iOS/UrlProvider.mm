extern "C" {
    // Helper method to create C string copy
    char* MakeStringCopy (NSString* nsstring)
    {
        if (nsstring == NULL) {
            return NULL;
        }
        // convert from NSString to char with utf8 encoding
        const char* string = [nsstring cStringUsingEncoding:NSUTF8StringEncoding];
        if (string == NULL) {
            return NULL;
        }

        // create char copy with malloc and strcpy
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        return res;
    }

    const char* _getSettingsURL () {
         NSURL * url = [NSURL URLWithString: UIApplicationOpenSettingsURLString];
         return MakeStringCopy(url.absoluteString);
    }

    void _openSettings () {
        NSURL * url = [NSURL URLWithString: UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL: url];
    }
}
