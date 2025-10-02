<?php

class OrderTransferAccessor {
	
	// https://www.ibenic.com/custom-tables-wordpress-deleting/
	// https://www.ibenic.com/custom-tables-wordpress-installing-updating/
	// https://www.ibenic.com/custom-tables-wordpress/

	// Do not delete table at plugin uninstall as data is then lost and can't be recovered. Do a button with are you sre you want to delete..

	public const TABLENAME = 'ordertransfersbiro';

	public static function Create() {
		global $wpdb;
   
		// Let's not break the site with exception messages
		$wpdb->hide_errors();
		
		if ( ! function_exists( 'dbDelta' ) ) {
			require_once ABSPATH . 'wp-admin/includes/upgrade.php';
		}
		
		$collate = '';
		
		if ( $wpdb->has_cap( 'collation' ) ) {
			$collate = $wpdb->get_charset_collate();
		}
		
		$some = OrderTransferAccessor::TABLENAME;
		$schema = "
		CREATE TABLE {$wpdb->prefix}{$some} (
		orderid varchar(50) NOT NULL,
		orderstatus varchar(20),
		ordertransferstatus int,
		error varchar(300),
		birokratdoctype varchar(20),
		birokratdocnum varchar(10),
		datecreated varchar(30),
		datelastmodified varchar(30),
		datevalidated varchar(30),
		PRIMARY KEY  (orderid, orderstatus)
		) $collate;";
		
		dbDelta( $schema );
	}

	public static function Drop() {
		global $wpdb;
		
		/**
		 * All custom tables we've made.
		 */
		$tables = array(
		  OrderTransferAccessor::TABLENAME
		);
		
		foreach( $tables as $table ) {
		  $wpdb->query( "DROP TABLE IF EXISTS {$wpdb->prefix}" . $table );
		}
	}

	public static function AddUnaccepted($ordertransfer) {
		global $wpdb;
		$some = OrderTransferAccessor::TABLENAME;
		$wpdb->insert("{$wpdb->prefix}{$some}", array(
			"orderid" => $ordertransfer['orderid'],
			"orderstatus" => $ordertransfer['orderstatus'],
			"ordertransferstatus" => '1',
			"datecreated" => $ordertransfer['datecreated'],
			"datelastmodified" => $ordertransfer['datecreated']
		 ));

	}

	public static function Delete($orderid, $orderstatus) {
		global $wpdb;
		$some = OrderTransferAccessor::TABLENAME;
		return $wpdb->delete("{$wpdb->prefix}{$some}", array("orderid"=>$orderid,"orderstatus"=>$orderstatus));
	}

	public static function Update($ordertransfer) {
		global $wpdb;
		$some = OrderTransferAccessor::TABLENAME;

		$updates = array();

		
		$updates['ordertransferstatus'] =  $ordertransfer['ordertransferstatus'];
		if (array_key_exists('birokratdoctype', $ordertransfer) && !is_null($ordertransfer['birokratdoctype']) && $ordertransfer['birokratdoctype'] != '') {
			$updates['birokratdoctype'] = $ordertransfer['birokratdoctype'];
		}
		if (array_key_exists('birokratdocnum', $ordertransfer) && !is_null($ordertransfer['birokratdocnum']) && $ordertransfer['birokratdocnum'] != '') {
			$updates['birokratdocnum'] = $ordertransfer['birokratdocnum'];
		}
		if (array_key_exists('error', $ordertransfer) && !is_null($ordertransfer['error'])) {
			$updates['error'] = $ordertransfer['error'];
		}
		if (array_key_exists('datelastmodified', $ordertransfer) && !is_null($ordertransfer['datelastmodified']) && $ordertransfer['datelastmodified'] != '') {
			$updates['datelastmodified'] = $ordertransfer['datelastmodified'];
		}
		if (array_key_exists('datevalidated', $ordertransfer) && !is_null($ordertransfer['datevalidated']) && $ordertransfer['datevalidated'] != '') {
			$updates['datevalidated'] = $ordertransfer['datevalidated'];
		}


		return $wpdb->update("{$wpdb->prefix}{$some}", 
			$updates,
			array("orderid" => $ordertransfer['orderid'],
				"orderstatus" => $ordertransfer['orderstatus']
			)
		);

	}

	public static function GetAll() {
		global $wpdb;
		$table = OrderTransferAccessor::TABLENAME;
		$results = $wpdb->get_results( "SELECT * FROM {$wpdb->prefix}{$table}", ARRAY_A);
		return $results;
	}

	public static function Get($orderid, $orderstatus) {
		global $wpdb;
		$table = OrderTransferAccessor::TABLENAME;
		$results = $wpdb->get_results( "SELECT * FROM {$wpdb->prefix}{$table} WHERE orderid = '{$orderid}' AND orderstatus = '{$orderstatus}'", ARRAY_A);
		return $results;
	}


}