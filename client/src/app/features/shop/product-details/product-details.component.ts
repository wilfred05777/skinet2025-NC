import { Component, inject, OnInit } from '@angular/core';
import { Product } from '../../../shared/models/products';
import { ActivatedRoute } from '@angular/router';
import { ShopService } from '../../../core/services/shop.service';

@Component({
  selector: 'app-product-details',
  imports: [],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss'
})
export class ProductDetailsComponent implements OnInit {
  private shopService = inject(ShopService);
  private activatedRoute = inject(ActivatedRoute);
  product?: Product;

  ngOnInit(): void {
    this.loadProduct();
  }

  loadProduct(){
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) return;
      {
      this.shopService.getProduct(+id).subscribe({
        next: product => this.product = product,
        error: error => console.error(error),
      });
    }
  }
}
