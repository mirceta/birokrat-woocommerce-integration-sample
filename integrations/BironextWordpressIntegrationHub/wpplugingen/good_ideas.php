
    // old way of on product updated
	add_action( 'added_post_meta', 'mp_sync_on_product_save', 10, 4 );
	add_action( 'updated_post_meta', 'mp_sync_on_product_save', 10, 4 );
	
	
	function mp_sync_on_product_save( $meta_id, $post_id, $meta_key, $meta_value ) {
		
	    if ( $meta_key == '_edit_lock' ) { // we've been editing the post
	        if ( get_post_type( $post_id ) == 'product' ) { // we've been editing a product
	            $product = wc_get_product( $post_id );
	            // do something with this product
				logInformation('ARTICLE DATA', array($meta_id, $post_id, $meta_key, $meta_value));
	        }
	    }
	}


	// ONLY AFTER woocommerce 3.0! - on product updated

	{
		add_action( 'woocommerce_update_product', 'mp_sync_on_product_save_new', 10, 1 );
		function mp_sync_on_product_save_new( $product_id ) {
		     $product = wc_get_product( $product_id );
		     // do something with this product
		}
	}
