<?php
////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////
// DEBUG HELP
	
function logInformation($message, $args, $level = "") {
/*
	$log = "---------------------------------------------\r\n";
		
	$error_dir = 'C:/xampp/htdocs/mircetadev/wp-content/plugins/birokrat/logs/log.txt';
	//echo_log(nl2br($log));
	error_log($log, 3, $error_dir);
	
	
	$LOG_DIVISION = array('NOTHING', 'L1');
	if (WP_DEBUG && ($level == "" || in_array( $level, $LOG_DIVISION ))) {
		$log = "---------------------------------------------\r\n";
		
		$theDate    = new DateTime('now');
		echo $stringDate = $theDate->format('Y-m-d H:i:s');
		
		$log = $log . "INFORMATION ENTRY\r\n";
		$log = $log . $stringDate . "\r\n";
		$log = $log . $message . "\r\n";
		
		$log = $log . 'ARGS' . "\r\n";
		foreach ($args as $value) {
			$log = $log . print_r($value, true) . "\r\n";
		}
		
		$trace = wp_debug_backtrace_summary();
		
		$log = $log . print_r($trace, true) . "\r\n";
		
		$log = $log . "---------------------------------------------\r\n";
		
		$error_dir = 'C:/xampp/htdocs/mircetadev/wp-content/plugins/birokrat/logs/log.txt';
		//echo_log(nl2br($log));
		error_log($log, 3, $error_dir);
	}
*/
}

function logError($message) {
	
}

function echo_log( $what )
{
    echo '<pre>'.print_r( $what, true ).'</pre>';
}

function deep_copy($obj) {
	return unserialize(serialize($obj));
}