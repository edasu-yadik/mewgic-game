#!/bin/sh
set -eu
umask 077

: "${MEWGIC_BACKUP_DIR:?Set a private directory on an encrypted filesystem}"
: "${RESTIC_REPOSITORY:?Set the encrypted off-site repository}"
: "${RESTIC_PASSWORD_FILE:?Set the path to the restricted restic password file}"
stamp=$(date -u +%Y%m%dT%H%M%SZ)
staging=$(mktemp -d "$MEWGIC_BACKUP_DIR/stage.XXXXXXXX")
snapshot="$staging/mewgic-$stamp.db"
trap 'rm -f -- "$snapshot"; rmdir -- "$staging"' EXIT
/usr/bin/dotnet /opt/mewgic/current/ClanServer.dll backup /var/lib/mewgic/game.db "$snapshot"
restic backup --tag mewgic-db --stdin --stdin-filename mewgic.db < "$snapshot"
