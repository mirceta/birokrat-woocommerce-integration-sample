<?php
class ProductTransferAccessor {
	
	// https://www.ibenic.com/custom-tables-wordpress-deleting/
	// https://www.ibenic.com/custom-tables-wordpress-installing-updating/
	// https://www.ibenic.com/custom-tables-wordpress/

	// Do not delete table at plugin uninstall as data is then lost and can't be recovered. Do a button with are you sre you want to delete..

	public const TABLENAME = 'producttransfersbiro';

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
		
		$some = ProductTransferAccessor::TABLENAME;
		$schema = "
		CREATE TABLE {$wpdb->prefix}{$some} (
		product_id varchar(30) NOT NULL,
		last_event int NOT NULL,
		last_event_success int NOT NULL,
		last_event_message varchar(300),
		last_event_datetime varchar(30),
		PRIMARY KEY  (product_id)
		) $collate;";
		
		dbDelta( $schema );
	}

	public static function Drop() {
		global $wpdb;
		
		/**
		 * All custom tables we've made.
		 */
		$tables = array(
		  ProductTransferAccessor::TABLENAME
		);
		
		foreach( $tables as $table ) {
		  $wpdb->query( "DROP TABLE IF EXISTS {$wpdb->prefix}" . $table );
		}
	}

	public static function AddOrUpdate($producttransfer) {
		global $wpdb;
		$some = ProductTransferAccessor::TABLENAME;

		return $wpdb->replace("{$wpdb->prefix}{$some}", array(
			"product_id" => $producttransfer['product_id'],
			"last_event" => intval($producttransfer['last_event']),
			"last_event_success" => intval($producttransfer['last_event_success']),
			"last_event_message" => $producttransfer['last_event_message'],
			"last_event_datetime" => $producttransfer['last_event_datetime']
		 ));
	}

	public static function Delete($productid) {
		global $wpdb;
		$some = ProductTransferAccessor::TABLENAME;
		return $wpdb->delete("{$wpdb->prefix}{$some}", array("product_id"=>$productid));
	}

	public static function GetAll($only_errors) {

		global $wpdb;
		$table = ProductTransferAccessor::TABLENAME;
		$query = "SELECT * FROM {$wpdb->prefix}{$table}";
		if ($only_errors) {
			$query = $query . " WHERE last_event_success <> 0";
		}
		$query = $query . ' order by last_event_datetime desc';
		
		
		$results = $wpdb->get_results( $query, ARRAY_A);
		return $results;
	}


}