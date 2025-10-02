<?php

$allordertransfers = array();
$currpageordertransfers = array();
$page = 0;
$disppage = 0;
$allpages = 0;
static $zapor = 0;
function birokrat_order_transfers_page() {
	
	global $currpageordertransfers;
	global $allordertransfers;
	global $page;
	global $allpages;
	global $disppage;
	global $zapor;

	

	$allordertransfers = OrderTransferAccessor::GetAll();
	
	// PAGINATION
	
	$allpages = floor(count($allordertransfers) / 30) + 1;

	if ( isset($_POST['page'])) {
		$page = $_POST['page'];
	}
	if ( isset($_POST['zapor'])) {
		$zapor = $_POST['zapor'];
	}

	if ( isset($_POST['nextpage'])) {
		$zapor = $zapor . 'N';
		if ($page < $allpages - 1) {
			$page = $page + 1;
		}
	}
	else if ( isset($_POST['lastpage'])) {
		$zapor = $zapor . 'L';
		$page = $allpages - 1;
	}
	else if ( isset($_POST['firstpage'])) {
		$zapor = $zapor . 'F';
		$page = 0;
	}
	else if ( isset($_POST['prevpage'])) {
		$zapor = $zapor . 'P';
		if ($page > 0)
			$page = $page - 1;
	}
	

	$currpageordertransfers = array();
	for ( $i = $page * 30; $i < min(($page + 1) * 30, count($allordertransfers)); $i++ ) {
		array_push($currpageordertransfers, $allordertransfers[$i]);
	}
	

	// HANDLE RESET CLICKS
	for ( $i = 0; $i < count($currpageordertransfers); $i++ ) {
		if (isset($_POST[$i]))
		{
			$zapor = $zapor . 'X';
			// reset
			$content = array();
			$content['orderid'] = $currpageordertransfers[$i]['orderid'];
			$content['orderstatus'] = $currpageordertransfers[$i]['orderstatus'];
			$content['ordertransferstatus'] = '1';
			$content['datelastmodified'] = date('Y-m-dh:i:s');
			$content['error'] = '';
		
			OrderTransferAccessor::Update($content);
			$currpageordertransfers = OrderTransferAccessor::GetAll();
			break;
		}
	}

	for ( $i = 0; $i < count($currpageordertransfers); $i++ ) {
		$dic = array(
			'1' => 'UNACCEPTED',
			'2' => 'ACCEPTED',
			'3' => 'ERROR',
			'4' => 'UNVERIFIED',
			'5' => 'VERIFIED',
			'6' => 'VERIFICATION_ERROR',
			'7' => 'NO_EVENT'
		);
		$backgroundColor = array(
			'1' => 'gray',
			'2' => 'yellow',
			'3' => 'red',
			'4' => 'teal',
			'5' => 'green',
			'6' => 'orange',
			'7' => 'silver'
		);
		$color = array(
			'1' => 'white',
			'2' => 'black',
			'3' => 'white',
			'4' => 'black',
			'5' => 'white',
			'6' => 'white',
			'7' => 'black'
		);
		$reset = array(
			'1' => false,
			'2' => true,
			'3' => true,
			'4' => true,
			'5' => true,
			'6' => true,
			'7' => true
		);
		$currpageordertransfers[$i]['backgroundcolor'] = $backgroundColor[$currpageordertransfers[$i]['ordertransferstatus']];
		$currpageordertransfers[$i]['color'] = $color[$currpageordertransfers[$i]['ordertransferstatus']];
		$currpageordertransfers[$i]['reset'] = $reset[$currpageordertransfers[$i]['ordertransferstatus']];
	
		$currpageordertransfers[$i]['ordertransferstatus'] = $dic[$currpageordertransfers[$i]['ordertransferstatus']];
	}


	require('birokrat_orders_page_html.php');
}