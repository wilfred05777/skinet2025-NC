import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { Product } from './shared/models/products';
import { ShopService } from './core/services/shop.service';
import { Pagination } from './shared/models/pagination';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  /* implement shop service */
  private shopService = inject(ShopService);

  /* refactor the code for service method
  baseUrl = 'https://localhost:5001/api/'
  private http = inject(HttpClient);
  */
  title = 'Skinet';
  products: Product[] = [];

  ngOnInit():void {

    // this.http.get<Pagination<Product>>(this.baseUrl + 'products').subscribe({

    this.shopService.getProducts().subscribe({
      // return service from shop.service.ts to fix the error
      next: response => this.products = response.data,
      error: error => console.log(error),

      // complete: () => console.log('Complete')
    })
  }
}
