# **Hamster**
<img src="/resources/hamster.png" alt="drawing" width="150" height="150"/>

### **Swiss army data backup solution**

I called it hamsters because Hamsters don't just stuff their faces with food; they can also pack up their cheek pouches with babies. Mother hamsters are protective of their babies, and when they perceive a threat to the newborn clan, she may stuff the babies into her mouth and hide the babies in her cheek ;)

So Hamster was born. She protects all our data from incidents!

Hamster needs a configuration file beside the executable, which contains the S3 secrets and backup configurations:

```
{
    "S3_EndpointURL": "https://s3.ir-thr-at1.arvanstorage.com",
    "S3_AccessKey": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "S3_SecretKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Operations":
    [
        {
            "Name": "Test",
            "UnixCommand": "echo Hi from Hamster! > $Name/Hmaster.txt",
            "WindowsCommand": "",
            "PersianDate": true,
            "BucketName": "$name-Backup",
            "RemoteFileName": "$Name-$Date.zip"
        },
        {
            "Name": "StreamServer-DB",
            "UnixCommand": "docker exec \"DB-SqlServer\" /opt/mssql-tools/bin/sqlcmd -b -V16 -S localhost -U SA -P P@ssw0rd -Q \"BACKUP DATABASE [Stream] TO DISK = N'/var/opt/mssql/backups/Backup.bak' with NOFORMAT, INIT, NAME = 'UserDB-full', SKIP\" && docker cp DB-SqlServer:/var/opt/mssql/backups/Backup.bak",
            "WindowsCommand": "",
            "PersianDate": true,
            "BucketName": "$Name-Backup",
            "RemoteFileName": "$Name-$Date.zip"
        },
        {
            "Name": "StreamServer-Media",
            "UnixCommand": "tar -czf $Name/StreamServer-Media.zip /media/StreamStorage",
            "WindowsCommand": "",
            "PersianDate": true,
            "BucketName": "$Name-Backup",
            "RemoteFileName": "$Name-$Date.zip"
        }
}
```

The \$Name and $Date is predefined variables which replaces at runtime with actual values, you can use them in Bucket name, Remote file name or Command field of operation.


> $Name => Replaced with operation name defined in Name field   


> $Date => Replaced with current date time : 2022.15.07-15.53.16 


* PersianDate boolean property indicate you want use Hijri calender instead of gregorian calender dates.

* Hamster creates a temp directory called \$Name. when the operation done the Hamster compress and uploads any content inside this directory as backup data to S3 storage, so remember to store your backups in \$Name directory.
  
The backup operation starts by passing the operation name defined in the config file to the Hamster executable:

```
sudo chmod +x hmaster-linux
hamster-linux StreamServer-DB
```

We use Jenkins in our CI/CD pipeline and use it to schedule the backup operations on multiple servers; this way, we don't need to copy and configure each server manually; we define a freestyle project for each server and set the schedule for the backup operation using CRON time and execute Hamster with the proper command line on the target server.

- If you use your CI/CD to schedule the backups, it’s necessary to add a post-build action to clean up the workspace on the server after each execution. This step is for your security to remove the config.json from the server, this file contains secrets to access server databases, files and others, and it shouldn’t stay there all the time.

I welcome your ideas and collaboration on development.
