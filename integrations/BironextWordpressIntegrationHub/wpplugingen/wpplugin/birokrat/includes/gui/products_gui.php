<?php

$allproducttransfers = array();
$currpageproducttransfers = array();
$productpage = 0;
$dispproductpage = 0;
$allproductpages = 0;
static $productzapor = 0;
$productonlyerror = true;

$tmparke = '';
function birokrat_product_transfers_page() {
	
	global $currpageproducttransfers;
	global $allproducttransfers;
	global $productpage;
	global $allproductpages;
	global $dispproductpage;
	global $productzapor;
	global $productonlyerror;
	global $tmparke;


	if ( isset($_POST['zapor'])) {
			$productzapor = $_POST['zapor'];
	}
	if ( isset($_POST['onlyerrors'])) {

		// onlyerrors is a checkbox. It's an array of values - checkbox has the attribute "value" - if the checkbox is checked, you will get the value of the "value" in one element of _POST['onlyerrors'] 

		if (count($_POST['onlyerrors']) > 0) {
			$productonlyerror = 'checked';
		} else {
			$productonlyerror = '';
		}
	} else {
		$productonlyerror = '';
	}

	$allproducttransfers = ProductTransferAccessor::GetAll($productonlyerror == 'checked');
	
	// PAGINATION
	
	$allproductpages = floor(count($allproducttransfers) / 30) + 1;


	
	if ( isset($_POST['page'])) {
		$productzapor = $productzapor . "";
		$productpage = $_POST['page'];
	}

	if ( isset($_POST['nextpage'])) {
		if ($productpage < $allproductpages - 1) {
			$productpage = $productpage + 1;
		}
	}
	else if ( isset($_POST['lastpage'])) {
		$productzapor = $productzapor . 'L';
		$productpage = $allproductpages - 1;
	}
	else if ( isset($_POST['firstpage'])) {
		$productzapor = $productzapor . 'F';
		$productpage = 0;
	}
	else if ( isset($_POST['prevpage'])) {
		$productzapor = $productzapor . 'P';
		if ($productpage > 0)
			$productpage = $productpage - 1;
	}
	

	$currpageproducttransfers = array();
	for ( $i = $productpage * 30; $i < min(($productpage + 1) * 30, count($allproducttransfers)); $i++ ) {
		array_push($currpageproducttransfers, $allproducttransfers[$i]);
	}

	for ( $i = 0; $i < count($currpageproducttransfers); $i++ ) {
		$sucmap = array(
			'0' => 'SUCCESSFUL',
			'1' => 'BIROKRAT_ERROR',
			'2' => 'INTEGRATION_ERROR',
			'3' => 'INTERNAL_ERROR',
		);

		$sucbackgroundColor = array(
			'0' => 'green',
			'1' => 'red',
			'2' => 'red',
			'3' => 'red'
		);
		$succolor = array(
			'0' => 'white',
			'1' => 'white',
			'2' => 'white',
			'3' => 'white'
		);

		$emap = array(
			'0' => 'ADD',
			'1' => 'SYNC'
		);
		$ebackgroundColor = array(
			'0' => 'teal',
			'1' => 'orange',
		);
		$ecolor = array(
			'0' => 'black',
			'1' => 'white',
		);
	
		
		$currpageproducttransfers[$i]['sucbackgroundcolor'] = $sucbackgroundColor[$currpageproducttransfers[$i]['last_event_success']];
		$currpageproducttransfers[$i]['succolor'] = $succolor[$currpageproducttransfers[$i]['last_event_success']];
		$currpageproducttransfers[$i]['ebackgroundcolor'] = $ebackgroundColor[$currpageproducttransfers[$i]['last_event']];
		$currpageproducttransfers[$i]['ecolor'] = $ecolor[$currpageproducttransfers[$i]['last_event']];

		// must be last!
		$currpageproducttransfers[$i]['last_event_success'] = $sucmap[$currpageproducttransfers[$i]['last_event_success']];
		$currpageproducttransfers[$i]['last_event'] = $emap[$currpageproducttransfers[$i]['last_event']];
	}

	require('birokrat_products_page_html.php');
}