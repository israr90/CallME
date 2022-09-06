<?php

// platform_check.php @generated by Composer

$issues = array();

if (!(PHP_VERSION_ID >= 50300 && PHP_VERSION_ID < 99999)) {
    $issues[] = 'Your Composer dependencies require a PHP version ">= 5.3.0" and "< 99999". You are running ' . PHP_VERSION  .  '.';
}

if ($issues) {
    echo 'Composer detected issues in your platform:' . "\n\n" . implode("\n", $issues);
    exit(104);
}
