<style>
/* Tooltip container */
.tooltip {
  position: relative;
  display: inline-block;
  border-bottom: 1px dotted black; /* If you want dots under the hoverable text */
}

/* Tooltip text */
.tooltip .tooltiptext {
  visibility: hidden;
  width: 300px;
  height: 300px;
  background-color: black;
  word-wrap:break-word;
  color: #fff;
  text-align: center;
  padding: 5px 0;
  border-radius: 6px;
 
  /* Position the tooltip text - see examples below! */
  position: absolute;
  z-index: 1;
}

/* Show the tooltip text when you mouse over the tooltip container */
.tooltip:hover .tooltiptext {
  visibility: visible;
}
</style>

<h2><?php _e( 'Birokrat plugin', 'WpAdminStyle' ); ?></h2>

<div class="wrap">

	<div id="icon-options-general" class="icon32"></div>
	<h1><?php esc_attr_e( 'History', 'WpAdminStyle' ); ?></h1>

	<div id="poststuff">

		<div id="post-body" class="metabox-holder columns-2">

			<!-- main content -->
			<div id="post-body-content">

				<div class="meta-box-sortables ui-sortable">

					<div class="postbox">

						<h2><span><?php esc_attr_e( 'Product transfers', 'WpAdminStyle' ); ?></span></h2>

						<div class="inside">
							<form method="post" action="">
								<label for="onlyerrors">
									<input type="checkbox" id="onlyerrors" name="onlyerrors[]" value="onlyerrors" <?php echo $productonlyerror; ?>/>
									<span><?php esc_attr_e( 'Show only products with errors', 'WpAdminStyle' ); ?></span>
									<input type='submit' value='SEARCH' name='only_errors_btn' class='last-page' href='#' />
								</label>
								<div class="tablenav">
									<div class="tablenav-pages">
										<!--<?php echo $productzapor; ?>!-->
										<input type="hidden" name="page" value=<?php echo $productpage; ?>>
										<input type="hidden" name="zapor" value=<?php echo $productzapor; ?>>
										<span class="displaying-num">Displaying <?php echo count($allproducttransfers); ?> items</span>
										<input type='submit' value = '<<' name='firstpage' class='last-page' href='#' />
										<input type='submit' value='<' name='prevpage' class='last-page' href='#' />
										<span><?php echo $productpage + 1; ?> of <span class='total-pages'><?php echo $allproductpages; ?></span></span>
										<input type='submit' value = '>' name='nextpage' class='last-page' href='#' />
										<input type='submit' value='>>'name='lastpage' class='last-page' href='#' />
									</div>
								</div>
								<table class="form-table">
									<tr>
										<th class="row-title"><?php esc_attr_e( 'Product Id', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Last Event', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Outcome', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Message', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'When', 'WpAdminStyle' ); ?></th>
									</tr>

									<?php for ( $i = 0; $i < count($currpageproducttransfers); $i++ ):?>
										<tr valign="top">
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageproducttransfers[$i]['product_id']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell" style='height:50px;color:<?php echo $currpageproducttransfers[$i]['ecolor']; ?>;background-color:<?php echo $currpageproducttransfers[$i]['ebackgroundcolor']; ?>;padding:5px;margin:5px;border-radius:5px;'>
													<?php echo $currpageproducttransfers[$i]['last_event']; ?>
												</label>
											</td>
											<td scope="row">
											</td>
											<td scope="row">
												<label for="tablecell" style='height:50px;color:<?php echo $currpageproducttransfers[$i]['succolor']; ?>;background-color:<?php echo $currpageproducttransfers[$i]['sucbackgroundcolor']; ?>;padding:5px;margin:5px;border-radius:5px;'>
													<?php echo $currpageproducttransfers[$i]['last_event_success']; ?>
												</label>
											</td>
											<td scope="row">
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageproducttransfers[$i]['last_event_message']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageproducttransfers[$i]['last_event_datetime']; ?>
												</label>
											</td>
										</tr>
									<?php endfor; ?>
								</table>
							</form>
						</div>
						<!-- .inside -->

					</div>
					<!-- .postbox -->

				</div>
				<!-- .meta-box-sortables .ui-sortable -->

			</div>
			<!-- post-body-content -->

		</div>
		<!-- #post-body .metabox-holder .columns-2 -->

		<br class="clear">
	</div>
	<!-- #poststuff -->

</div> <!-- .wrap -->