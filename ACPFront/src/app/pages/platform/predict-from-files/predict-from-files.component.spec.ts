import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PredictFromFilesComponent } from './predict-from-files.component';

describe('PredictFromFilesComponent', () => {
  let component: PredictFromFilesComponent;
  let fixture: ComponentFixture<PredictFromFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PredictFromFilesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PredictFromFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
