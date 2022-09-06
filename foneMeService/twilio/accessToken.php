<?php
/*
 * Creates an access token with VoiceGrant using your Twilio credentials.
 */
include('./vendor/autoload.php');
include('./config.php');
use Twilio\Jwt\AccessToken;
use Twilio\Jwt\Grants\VoiceGrant;

// Use identity and room from query string if provided
$identity = isset($_GET["identity"]) ? $_GET["identity"] : NULL;
if (!isset($identity) || empty($identity)) {
  $identity = isset($_POST["identity"]) ? $_POST["identity"] : "alice";
}

// Create access token, which we will serialize and send to the client
/*$token = new AccessToken($ACCOUNT_SID, 
                         $API_KEY, 
                         $API_KEY_SECRET, 
                         3600, 
                         $identity
);*/
$token = "NAN";

// Grant access to Video
$grant = new VoiceGrant();
$grant->setOutgoingApplicationSid($APP_SID);
$grant->setPushCredentialSid($PUSH_CREDENTIAL_SID);
//$token->addGrant($grant);

echo "Sorry, this service is not enabled!";
//echo $token->toJWT();
