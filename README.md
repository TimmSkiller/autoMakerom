# autoMakerom
Console application that converts 3DS CDN Contents to .CIA using makerom, ninfs, and a provided title key CSV

Requirements:
- CDN Contents in the same structure they originally came in, like this, for example:
```
Folder(can be any name)\
  0004000000168124\
    tmd
    00000000
    00000001
  0004000000148154\
    tmd
    00000000
    00000001
  0004000000166124\
    tmd
    00000000
    00000001
```

- A file (in CSV structures as `TitleID,DecKey`) for the required titlekeys to decrypt & build CIAs for the contents

Usage:
`autoMakerom CDN_CONTENT_DIR PATH_TO_KEY_CSV`

After autoMakerom has finished, you might see two folders called`success_builds` and `failed_mounts`. The first one contains the successfully built CIAs that had no issues while they were processed. In the second however, you will see another two folders, called `nokey` and/or `failed_makerom_build`. The first one will contain empty files with Title ID's as their names for which there were no decryption keys found within the specified key CSV. The second one will contain empty files with Title ID's as their names which makerom could not successfully build. In that case, you must check in the console log for that specific content to see where it went wrong.
