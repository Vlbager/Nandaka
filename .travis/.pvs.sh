#!/bin/bash

pvs_install() {
    wget -q -O - https://files.viva64.com/etc/pubkey.txt \
      | sudo apt-key add -
    sudo wget -O /etc/apt/sources.list.d/viva64.list \
      https://files.viva64.com/etc/viva64.list  
    
    sudo apt-get update -qq
    sudo apt-get install -qq pvs-studio \
                             libio-socket-ssl-perl \
                             libnet-ssleay-perl
}

pvs_analyze() {
    pvs-studio-analyzer credentials $PVS_USERNAME $PVS_KEY -o PVS-Studio.lic
    pvs-studio-analyzer analyze -j2 -l PVS-Studio.lic \
                                -o PVS-Studio-${CC}.log \
                                --disableLicenseExpirationCheck

    plog-converter -t html PVS-Studio-${CC}.log -o PVS-Studio-${CC}.html
    sendemail -t mail@domain.com \
              -u "PVS-Studio $CC report, commit:$TRAVIS_COMMIT" \
              -m "PVS-Studio $CC report, commit:$TRAVIS_COMMIT" \
              -s smtp.gmail.com:587 \
              -xu $MAIL_USER \
              -xp $MAIL_PASSWORD \
              -o tls=yes \
              -f $MAIL_USER \
              -a PVS-Studio-${CC}.log PVS-Studio-${CC}.html
}

set -e
set -x

$1;