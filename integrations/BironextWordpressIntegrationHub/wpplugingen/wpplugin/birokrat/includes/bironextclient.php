<?php

function sendHttpPostRequest($endpoint, $jsonpayload) {
		
	// THIS FUNCTION IN DEPENDENT ON WORDPRESS OPTIONS!!!
	$options = get_option( 'wporg_options' );
	
	$apikey = $options['wporg_field_bironext_api_key'];
	$address = $options['wporg_field_bironext_address'];
	
	logInformation('Request start', array($address, $endpoint, $jsonpayload));
	
	$ch = curl_init();
	curl_setopt($ch, CURLOPT_URL, $address . $endpoint);
	curl_setopt($ch, CURLOPT_POST, 1);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
	curl_setopt($ch, CURLOPT_HEADER, FALSE);
	curl_setopt($ch,CURLOPT_CONNECTTIMEOUT, 5);
	curl_setopt($ch,CURLOPT_TIMEOUT, 45);
	
	curl_setopt($ch, CURLOPT_HTTPHEADER, array(
	  "Content-Type: application/json",
	  "X-API-KEY: " . $apikey
	));
	curl_setopt( $ch, CURLOPT_POSTFIELDS, $jsonpayload );
	
	$response = curl_exec($ch);
	curl_close($ch);
	logInformation('Response start', array($response));
	return $response;
}