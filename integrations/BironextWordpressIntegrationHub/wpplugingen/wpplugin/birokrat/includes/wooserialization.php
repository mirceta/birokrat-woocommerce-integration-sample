<?php
// IT IS VERY IMPORTANT THAT THESE FUNCTIONS ARE COMPLETELY GENERIC!
function create_serializable_order($order) {
		
	$data = $order->get_data();
	$items = array(count($order->get_items()));
	
	$x = 0;
	foreach ($order->get_items() as $value) {
		$items[$x] = create_serializable_order_spec($value);
		$x = $x + 1;
	}
	
	$json = array();
	$json['data'] = $data;
	$json['items'] = $items;
	$json['coupons'] = get_coupons($order);
	$json['shipping_method'] = $order->get_shipping_method();
 	$json['used_coupons_codes'] = $order->get_coupon_codes(); // $order->get_used_coupons() bellow WC 3.7


	/* https://woocommerce.com/document/eu-vat-number/?_gac=1.16667076.1643793436.Cj0KCQiA9OiPBhCOARIsAI0y71CACmt6D73l47mq-R6pkhXPdDrbZMEBgkqb6C4j7YjTjhQ8Ymz58qYaAmEEEALw_wcB
	try {
		$json['vat_num'] = wc_eu_vat_get_vat_from_order($order);
	} catch (Exception $e) {
		$json['vat_num'] = "";
	}*/
	return $json;
}

function create_serializable_order_spec($item) {
	$item = $item->get_data();
		
	$product = wc_get_product($item['product_id']);
	
	$item['origin_product'] = create_serializable_product($product);
	return $item;
}

function create_serializable_product($product) {
	$serializable = $product->get_data();
	
	if ($product->is_type( 'variable' )) {
		$serializable['variations'] = $product->get_available_variations();
	}
	return $serializable;
}

function add_variable_product_variables_to_item_data($product, $item) {
	if ($product->is_type( 'variable' )) {
		foreach ($product->get_available_variations() as $key => $variation) {
			foreach ($variation['attributes'] as $attribute => $term_slug) {
				$taxonomy = str_replace('attribute_', '', $attribute);
				$attr_label_name = wc_attribute_label( $taxonomy );
        		$term_name = get_term_by( 'slug', $term_slug, $taxonomy )->name;
				$item_taxonomy_key = $item['meta_data'][0]->get_data()['key'];
				$item_taxonomy_val = $item['meta_data'][0]->get_data()['value'];
				logInformation('PRODUCT VARIATION DATA', array($taxonomy, $item_taxonomy_key, $term_slug, $item_taxonomy_val), 'L1');
				if ($taxonomy == $item_taxonomy_key && $term_slug == $item_taxonomy_val) { // $term_name instead of $term_slug??
					$item['additional_data'][$attr_label_name] = $item_taxonomy_val;
					logInformation('ITEM', json_encode($item), 'EVENTS');
					return $item;
				}
			}
		}
	}
	return $item;
}

function get_coupons($order) {
	$args = array(
	    'posts_per_page'   => -1,
	    'orderby'          => 'title',
	    'order'            => 'asc',
	    'post_type'        => 'shop_coupon',
	    'post_status'      => 'publish',
	);
	    
	$coupon_posts = get_posts( $args );
	$coupons = array();
	foreach ( $coupon_posts as $coupon_post ) {
	    $coupon = new WC_Coupon( $coupon_post->post_title );
	    array_push( $coupons, $coupon->get_data() );
	}
	logInformation('COUPONS', array($coupons), 'L1');
	return $coupons;
}

function get_all_products() {
    return array_map('wc_get_product', get_posts(['post_type'=>'product','nopaging'=>true]));
}