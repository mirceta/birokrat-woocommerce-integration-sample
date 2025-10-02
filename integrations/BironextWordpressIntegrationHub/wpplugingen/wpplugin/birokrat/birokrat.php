<?php

/**
 * Plugin Name: Birokrat plugin
 * Plugin URI: https://woocommerce.com/
 * Description: The official Birokrat integration plugin.
 * Version: 1.0.0
 * Author: Kristijan Mirceta
 * Author URI: https://woocommerce.com
 * Text Domain: woocommerce
 * Domain Path: /i18n/languages/
 * Requires at least: 5.4
 * Requires PHP: 7.0
 *
 * @package Birokrat
 */

// don't call the file directly
defined( 'ABSPATH' ) or die( 'Nope, not accessing this' );

/**
 * Check if WooCommerce is active
 **/
if ( !in_array( 'woocommerce/woocommerce.php', apply_filters( 'active_plugins', get_option( 'active_plugins' ) ) ) ) { exit; }
   
require_once('includes/utils.php');
require_once('includes/wooserialization.php');
require_once('includes/bironextclient.php');
require_once('includes/ordertransferaccessor.php');
require_once('includes/producttransferaccessor.php');
require_once('includes/gui/gui.php');



class BirokratPlugin {
	private static $birokrat;
	
	private function __construct() {
		
		//ProductTransferAccessor::Drop();
		//OrderTransferAccessor::Drop();
		OrderTransferAccessor::Create();
		ProductTransferAccessor::Create();
		
     	//register_activation_hook( __FILE__, array('BirokratInstaller', "activate") );

		$this->constants(); // Defines any constants used in the plugin
		$this->init();   // Sets up all the actions and filters
	}
	
	public static function getInstance() {
		if ( !self::$birokrat ) {
			self::$birokrat = new BirokratPlugin();
		}

		return self::$birokrat;
	}

	private function constants() {
	}

	private function init() {
		
		add_action('admin_menu', 'birokrat_menu');



		$opts = array();
		/*********
		$opts['wporg_field_bironext_api_key'] = [![![APIKEY]!]!]
		$opts['wporg_field_bironext_address'] = [![![SERVERADDRESS]!]!]
		*********/
		$opts['orderstransfers'] = array();
		$option_name = 'wporg_options';
		
		
		logInformation('ERROR ON ADD OPTION', array($opts), 'WP_EVENTS');	
		
		if (!get_option($option_name, false)) {
			if (!add_option($option_name, $opts)) {
				logInformation('ERROR ON ADD OPTION', array($opts), 'WP_EVENTS');	
			}
			
		} else {
			if (!update_option($option_name, $opts)) {
				logInformation('ERROR ON UPDATE OPTION', array($opts), 'WP_EVENTS');
			}
		}
		
		// new
		add_action('rest_api_init', array($this, 'register_routes'));
		
		// events
		/*********
		[![![PRODUCTHOOKS]!]!]
		[![![ORDERSTATUSHOOKS]!]!]
		[![![ATTACHMENTHOOK]!]!]
		*********/
	}	
	
	// <NEW>
	// #region [REST API EXTENSIONS]
	function set_order_transfer_state( WP_REST_Request $request ) {
		
		// update order transfer
		
		$query_params = $request->get_query_params();

		$map = array(
			'orderid', 
			'orderstatus',
			'ordertransferstatus',
			'except',
		    'birokratdoctype', 
		    'birokratdocnum',
		    'datelastmodified',
			'datevalidated'
		);
		$content = array();

		if (array_key_exists('except', $query_params)) {
			$tmp = $query_params['except'];
			$tmp = str_replace("?", "", $tmp);
			$content['error'] = $tmp;
		}
		
		for ( $i = 0; $i < count($map); $i++ ) { 
			if (array_key_exists($map[$i], $query_params) && !is_null($query_params[$map[$i]]) && !empty($query_params[$map[$i]])) {
				
				$tmp = $query_params[$map[$i]];
				$tmp = str_replace("?", "", $tmp);
				$content[$map[$i]] = $tmp;
				
			}
		}

		return OrderTransferAccessor::Update($content) == 1;
	}

	function get_order_transfer_states( WP_REST_Request $request ) {
		return OrderTransferAccessor::GetAll();
	}

	function get_order_transfer_state( WP_REST_Request $request ) {
		return OrderTransferAccessor::Get($request['orderid'], $request['orderstatus']);
	}

	function my_get_order(  WP_REST_Request $request ) {
		$parameters = $request->get_params();
		$order = wc_get_order($parameters['id']);
		$json = create_serializable_order($order);
		return $json;
		//return wc_get_order('22')->get_data(); 		
	}

	function add_unaccepted( WP_REST_Request $request ) {
		$parameters = $request->get_params();
		
		$content['orderid'] = str_replace("?", "", $parameters['orderid']);
		$content['orderstatus'] = str_replace("?", "", $parameters['orderstatus']);
		$content['datecreated'] = date('Y-m-dh:i:s');
		OrderTransferAccessor::AddUnaccepted($content);
		return '';
	}

	function delete_order_transfer_state( WP_REST_Request $request ) {
		$parameters = $request->get_params();
		$orderid = str_replace("?", "", $parameters['orderid']);
		$orderstatus = str_replace("?", "", $parameters['orderstatus']);
		return OrderTransferAccessor::Delete($orderid, $orderstatus);
	}

	function post_product_transfer( WP_REST_Request $request ) {
		$parameters = $request->get_params();
		
		$product_id = str_replace("?", "", $parameters['product_id']);
		$last_event = str_replace("?", "", $parameters['last_event']);
		$last_event_success = str_replace("?", "", $parameters['last_event_success']);
		$last_event_message = str_replace("?", "", $parameters['last_event_message']);
		$last_event_datetime = str_replace("?", "", $parameters['last_event_datetime']);

		$content = array();
		$content['product_id'] = $product_id;
		$content['last_event'] = $last_event;
		$content['last_event_success'] = $last_event_success;
		$content['last_event_message'] = $last_event_message;
		$content['last_event_datetime'] = $last_event_datetime;

		return ProductTransferAccessor::AddOrUpdate($content) == 1;
	}

	function get_product_transfers( WP_REST_Request $request ) {
		$parameters = $request->get_params();
		
		return ProductTransferAccessor::GetAll(false);
	}

	function delete_product_transfer( WP_REST_Request $request ) {
		$parameters = $request->get_params();
		$product_id = str_replace("?", "", $parameters['product_id']);
		return ProductTransferAccessor::Delete($product_id);
	}

	function get_products_by_sku( WP_REST_Request $request ) {

		$parameters = $request->get_params();
		$sku = str_replace("?", "", $parameters['sku']);
		return wc_get_product_id_by_sku($sku);
	}

	function register_routes() {
		register_rest_route( 'wc/v3', '/my_getorder/(?P<id>\d+)',
                  array(
                    'methods' => 'GET',
                    'callback' => array( $this, 'my_get_order')
                  )
    	);
		register_rest_route( 'wc/v3', '/my_ordertransfer/get',
				array(
				'methods' => 'GET',
				'callback' => array( $this, 'get_order_transfer_states')
				)
		);
		register_rest_route( 'wc/v3', '/my_ordertransfer/get',
				array(
				'methods' => 'POST',
				'callback' => array( $this, 'get_order_transfer_state')
				)
		);
		register_rest_route( 'wc/v3', '/my_ordertransfer/set',
				array(
				'methods' => 'PUT',
				'callback' => array( $this, 'set_order_transfer_state')
				)
		);
		register_rest_route( 'wc/v3', '/my_ordertransfer/unaccepted',
				array(
				'methods' => 'PUT',
				'callback' => array( $this, 'add_unaccepted')
				)
		);
		register_rest_route( 'wc/v3', '/my_ordertransfer/delete',
				array(
				'methods' => 'DELETE',
				'callback' => array( $this, 'delete_order_transfer_state')
				)
		);
		register_rest_route( 'wc/v3', '/my_producttransfer/set',
				array(
				'methods' => 'PUT',
				'callback' => array( $this, 'post_product_transfer')
				)
		);
		register_rest_route( 'wc/v3', '/my_producttransfer/get',
				array(
				'methods' => 'GET',
				'callback' => array( $this, 'get_product_transfers')
				)
		);
		register_rest_route( 'wc/v3', '/my_producttransfer/delete',
				array(
				'methods' => 'DELETE',
				'callback' => array( $this, 'delete_product_transfer')
				)
		);
		register_rest_route( 'wc/v3', '/my_products',
				array(
				'methods' => 'GET',
				'callback' => array( $this, 'get_products_by_sku')
				)
		);
	}
	// #endregion
	// </NEW>

	// #region [EVENTS]
	function onTurnOnSyncProducts() {
		
	}

	function onEmailIntercepted($attachments, $email_id, $order) {
		// Avoiding errors and problems
		if ( ! is_a( $order, 'WC_Order' ) || ! isset( $email_id ) ) {
			return $attachments;
		}

		$status = $order->get_status();
		$acceptable_statuses = array();
		/*********
		$acceptable_statuses = [![![ATTACHMENT_ACCEPTABLE_STATUSES]!]!];
		*********/
		if (in_array($status, $acceptable_statuses)) {
		
			$json = create_serializable_order($order);
			$response = sendHttpPostRequest('main/on-attachment-request', json_encode($json));
		
		
			$base64data = base64_decode($response, false);
			$upload_dir   = wp_upload_dir(); 
			if ( ! empty( $upload_dir['basedir'] ) ) {
				$user_dirname = $upload_dir['basedir'].'/invoice.pdf';
				file_put_contents($user_dirname, $base64data);
				$attachments[] = $user_dirname;
			} else {
				$response = sendHttpPostRequest('main/ping', '');
			}
		
			logInformation('EMAIL RESPONSE', array($response), 'L1');
		}
		return $attachments;
	}
	
	function onProductAdded($post_id) {
		$response = sendHttpPostRequest('main/on-article-added-raw', json_encode($post_id));
	}
	
	function onProductChanged($post_id) {
		$response = sendHttpPostRequest('main/on-article-changed-raw', json_encode($post_id));
	}
	
	function onProductDeleted($product) {
		
	}
	
	function onOrderStatusChanged( $arg1 ) {
		logInformation('on order status changed', array(), '');
		$order = wc_get_order($arg1);
		$json = create_serializable_order($order);

		$content = array();
		$content['orderid'] = $order->get_id();
		$content['orderstatus'] = $order->get_status();
		$content['datecreated'] = date('Y-m-dh:i:s');
		OrderTransferAccessor::AddUnaccepted($content);
		
		$response = sendHttpPostRequest('main/on-order-status-changed', json_encode($json));
	}
	// #endregion
	
	////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////
	// WORDPRESS EVENT SETUP
	
	// article
	// before woocommerce 3.0
	
	
	function wpse_110037_new_posts($new_status, $old_status, $post) {
	 	logInformation('POSTS EVENT', array(), '');
		if (!empty($post->ID) && in_array( $post->post_type, array( 'product') )) {
			if ($old_status != 'publish' && $new_status == 'publish') {
				onAddNewProduct($post->ID);		
			} else if ($new_status == 'trash') {
				// on delete
			} 
			else {
				if (!(in_array($old_status, array('new', 'auto-draft')) || in_array($new_status, array('new', 'auto-draft')))) {
					afterModifyExistingProduct($post->ID);
				}
				else {
					// new post draft
				}
			}
		}
			
	}
	
	function onAddNewProduct($post_id) {
		/* Optional full data
		logInformation('on Add new product', array(), '');
		$product = wc_get_product( $post_id );
		onProductAdded($product);
		*/
	}
	
	function afterModifyExistingProduct($post_id) {
		/* Optional full data
		logInformation('modify existing product', array(), '');
		$old_product_data = get_option('last_edited_product2');
		$new_product = wc_get_product($post_id);
		onProductChanged($old_product_data, $new_product);
		*/
	}
	
	function beforeOperationOnProduct($post_id, $data) {
		logInformation('before operation on product', array(), '');
		$post_type = $data->post_type;
		if (!$post_type) {
			$post_type = $data['post_type'];
		}
		
		if ($post_type == 'product') {
				
			$original = wc_get_product( $post_id );
			$product = deep_copy($original->get_data());
			//$product['post_id'] = $post_id;
			
			$option_name = 'last_edited_product2';
			
			if (!get_option($option_name, false)) {
				if (!add_option($option_name, $product)) {
					logInformation('ERROR ON ADD OPTION', array($product), 'WP_EVENTS');	
				}
				
			} else {
				if (!update_option($option_name, $product)) {
					logInformation('ERROR ON UPDATE OPTION', array($product), 'WP_EVENTS');
				}
			}
			logInformation('PREUPDATE', array($product['name'], get_option($option_name)), 'WP_EVENTS');
		}
		else if ($post_type == 'shop_order') {
			// set up perhaps for future use
		}
	}
}
$birokrat = BirokratPlugin::getInstance();